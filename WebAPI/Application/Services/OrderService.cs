using Application.IServices;
using Application.ViewModel;
using Application.ViewModel.Request;
using Application.ViewModel.Response;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public OrderService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<ApiResp> CreateTicketOrder(OrderRequest request)
        {
            ApiResp apiResp = new ApiResp();
            try
            {
                //gia promotion giam
                var promotion = await _uow.PromotionRepo.GetPromotionById(request.PromotionId);
                decimal? discountPercent = 1;
                if(promotion != null)
                {
                    discountPercent = promotion.DiscountPercent;
                }


                //lay list snack bang id tu request

                //lay list snackCombo

                //lay list seatSchedules bang id tu request
                List<SeatSchedule> seatSchedules = new();
                if (request.SeatScheduleId == null || !request.SeatScheduleId.Any())
                {
                    return apiResp.SetBadRequest("SeatScheduleId cannot be null or empty.");
                }
                foreach (var seatId in request.SeatScheduleId)
                {
                    var seatSchedule = await _uow.SeatScheduleRepo.GetAsync(x => x.Id == seatId);
                    if (seatSchedule != null)
                        seatSchedules.Add(seatSchedule);
                }

                decimal price = await CalculatePriceAsync(request.SeatScheduleId ,request.Snack, request.SnackCombo); // gia chua giam tu promotion

                var seatSchedulesMapped = _mapper.Map<List<SeatScheduleForOrderResponse>>(seatSchedules);
                OrderResponse rp = new OrderResponse
                {
                    UserId = request.UserId,
                    OrderTime = DateTime.UtcNow,
                    TotalAmount = price,
                    TotalAfter = price * discountPercent, // gia da giam
                    SeatSchedules = seatSchedulesMapped,
                    Snacks = request.Snack,
                    SnackCombos = request.SnackCombo,
                    Status = OrderEnum.Pending,
                };
                Order order = _mapper.Map<Order>(rp);
                await _uow.OrderRepo.AddAsync(order);

                rp.Id = order.Id;

                Payment payment = new Payment
                {
                    userId = order.UserId,
                    PaymentMethod = request.PaymentMethod,
                    PaymentTime = null,
                    AmountPaid = rp.TotalAfter,
                    OrderId = order.Id,
                    //SubscriptionId = 
                };
                await _uow.PaymentRepo.AddAsync(payment);

                await HoldSeatAsync(request.SeatScheduleId, request.UserId, null);

                await _uow.SaveChangesAsync();
                return apiResp.SetOk(rp);
            }catch (Exception ex)
            {
                return apiResp.SetBadRequest(ex.Message);
            }
        }

        public async Task<ApiResp> ViewTicketOrder(int page, int size)
        {
            ApiResp apiResp = new ApiResp();
            try
            {
                var rs = await _uow.OrderRepo.GetAllAsync(x => x.IsDeleted == false, null, page, size);
                if(rs != null)
                {
                    return apiResp.SetOk(rs);
                }
                else
                {
                    return apiResp.SetNotFound("Not found");
                }
            }catch(Exception ex)
            {
                return apiResp.SetBadRequest(ex);
            }
        }

        public async Task<ApiResp> ViewTicketOrderByUserId(Guid userId)
        {
            ApiResp apiResp=new ApiResp();
            try
            {
                var ticket = await _uow.OrderRepo.GetAsync(x => x.UserId == userId);
                if(ticket != null)
                {
                    return apiResp.SetOk(ticket);
                }
                else
                {
                    return apiResp.SetNotFound("Not found");
                }
            }catch(Exception ex)
            {
                return apiResp.SetBadRequest(ex.Message);
            }
        }

        public async Task<ApiResp> CancelTicketOrderById(List<Guid> seatScheduleId, Guid orderId)
        {
            ApiResp apiResp = new ApiResp();
            try
            {
                var order = await _uow.OrderRepo.GetAsync(x => x.Id == orderId && x.IsDeleted == false);
                order.Status = OrderEnum.Faild;
                if (!seatScheduleId.Any())
                {
                    return apiResp.SetNotFound("Not found");
                }
                var seats = await _uow.SeatScheduleRepo.GetAllAsync(s => seatScheduleId.Contains(s.Id));

                foreach(var seatSchedule in seats)
                {
                    seatSchedule.Status = SeatBookingStatus.Available;
                    await _uow.SeatScheduleRepo.UpdateAsync(seatSchedule);
                }
                await _uow.SaveChangesAsync();
                return apiResp.SetOk("Seat changed to Available");
                
            }
            catch(Exception ex)
            {
                return apiResp.SetBadRequest(ex.Message);
            }
        }
        public async Task<ApiResp> SuccessOrder(List<Guid> seatScheduleId, Guid orderId)
        {
            ApiResp apiResponse = new ApiResp();
            try
            {
                var order = await _uow.OrderRepo.GetAsync(x => x.Id == orderId && x.IsDeleted == false);
                order.Status = OrderEnum.Success;
                if (!seatScheduleId.Any())
                {
                    return apiResponse.SetNotFound("Not found");
                }
                var seats = await _uow.SeatScheduleRepo.GetAllAsync(s => seatScheduleId.Contains(s.Id));

                foreach (var seatSchedule in seats)
                {
                    seatSchedule.Status = SeatBookingStatus.Booked;
                    await _uow.SeatScheduleRepo.UpdateAsync(seatSchedule);
                }
                await _uow.SaveChangesAsync();
                return apiResponse.SetOk("Seat changed to Booked");
            }
            catch(Exception ex)
            {
                return apiResponse.SetBadRequest(ex.Message);
            }
        }

        private async Task<decimal> CalculatePriceAsync(IEnumerable<Guid>? seatScheduleIds, IEnumerable<SnackOrderRequest>? snackOrders, IEnumerable<SnackComboOrderRequest>? snackComboOrders)
        {
            decimal total = 0m;

            /* --------- 1. Giá ghế ---------- */
            if (seatScheduleIds?.Any() == true)
            {
                // Bảng giá theo loại ghế
                var seatTypePrices = (await _uow.SeatTypePriceRepo.GetAllAsync(null))
                                     .ToDictionary(x => x.SeatType, x => x.DefaultPrice);

                // Lấy SeatSchedule kèm Seat
                var schedules = await _uow.SeatScheduleRepo.GetAllAsync(
                                    ss => seatScheduleIds.Contains(ss.Id),
                                    include: q => q.Include(ss => ss.Seat!));

                foreach (var ss in schedules)
                {
                    if (ss.Seat != null &&
                        seatTypePrices.TryGetValue(ss.Seat.SeatType, out var price))
                    {
                        total += price;
                    }
                }
            }

            /* --------- 2. Snack lẻ ---------- */
            if (snackOrders?.Any() == true)
            {
                var snackIds = snackOrders.Select(o => o.SnackId).Distinct().ToList();
                var snacks = await _uow.SnackRepo.GetAllAsync(s => snackIds.Contains(s.Id));

                // Nhân giá * số lượng
                total += snacks.Sum(s =>
                {
                    var qty = snackOrders.First(o => o.SnackId == s.Id).Quantity;
                    return s.Price * qty;
                });
            }

            /* --------- 3. Snack combo -------- */
            if (snackComboOrders?.Any() == true)
            {
                var comboIds = snackComboOrders.Select(o => o.SnackComboId).Distinct().ToList();
                var combos = await _uow.SnackComboRepo.GetAllAsync(c => comboIds.Contains(c.Id));

                total += combos.Sum(c =>
                {
                    var qty = snackComboOrders.First(o => o.SnackComboId == c.Id).Quantity;
                    return c.TotalPrice * qty;
                });
            }

            return total;

        }

        public async Task<IEnumerable<SeatScheduleResponse>> HoldSeatAsync(List<Guid> seatIds, Guid? userId, string connectionId)
        {
            if (seatIds == null || seatIds.Count is < 1 or > 8)
                return Enumerable.Empty<SeatScheduleResponse>();

            var now = DateTime.UtcNow;

            // Bắt đầu transaction
            await using var tx = await _uow.BeginTransactionAsync();

            // Lấy các SeatSchedule theo danh sách ghế và suất chiếu
            var seats = await _uow.SeatScheduleRepo.GetAllAsync(s => seatIds.Contains(s.Id));

            if (seats == null || !seats.Any())
                return Enumerable.Empty<SeatScheduleResponse>();

            var succeededSeats = new List<SeatSchedule>();

            foreach (var seat in seats)
            {
                var isExpired = seat.Status == SeatBookingStatus.Hold && seat.HoldUntil < now;

                if (seat.Status == SeatBookingStatus.Hold && seat.HoldByUserId != userId && !isExpired)
                {
                    await tx.RollbackAsync();
                    throw new ApplicationException($"Seat {seat.Id} is already held by another user.");
                }

                // Chỉ cho giữ nếu ghế đang available hoặc hold đã hết hạn
                if (seat.Status == SeatBookingStatus.Available || isExpired)
                {
                    seat.Status = SeatBookingStatus.Hold;
                    seat.HoldUntil = now.AddMinutes(5);
                    seat.HoldByUserId = userId;
                    seat.HoldByConnectionId = connectionId;

                    succeededSeats.Add(seat);
                }
            }

            // Nếu không có ghế nào được giữ thành công → không cần save
            if (!succeededSeats.Any())
            {
                await tx.RollbackAsync();
                return Enumerable.Empty<SeatScheduleResponse>();
            }

            try
            {
                await _uow.SaveChangesAsync();   // vẫn trong transaction
                await tx.CommitAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                await tx.RollbackAsync();               // có xung đột → huỷ
                throw new InvalidOperationException("Someone hold this/these seat before. Please try again.");
            }

            // Map kết quả trả về
            var result = _mapper.Map<List<SeatScheduleResponse>>(succeededSeats);
            foreach (var item in result)
            {
                item.IsOwnedByCaller = true;
            }

            return result;
        }

    }
}
