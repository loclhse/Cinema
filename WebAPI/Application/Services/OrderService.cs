using Application.IServices;
using Application.ViewModel;
using Application.ViewModel.Request;
using Application.ViewModel.Response;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
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
                decimal? discount = 1;
                var promotion = await _uow.PromotionRepo.GetPromotionById(request.PromotionId);
                if(promotion != null)
                {
                    discount = promotion.DiscountPercent;
                }

                List<SeatSchedule> seatSchedules = new();
                if (request.SeatScheduleId != null)
                {
                    foreach (var id in request.SeatScheduleId)
                    {
                        var item = await _uow.SeatScheduleRepo.GetAsync(x => x.Id == id && x.IsDeleted == false);
                        if (item != null)
                            seatSchedules.Add(item);
                    }
                }

                // 2. Tạo Order trước (chưa thêm SnackOrder)
                var orderId = Guid.NewGuid();

                var total = await CalculatePriceAsync(request.SeatScheduleId, request.SnackOrders, request.SnackComboOrders);

                Order order = new Order
                {
                    Id = orderId,
                    UserId = request.UserId,
                    PaymentMethod = request.PaymentMethod,
                    OrderTime = DateTime.UtcNow,
                    TotalAmount = total * discount,
                    Status = OrderEnum.Pending,
                    SeatSchedules = seatSchedules
                };

                await _uow.OrderRepo.AddAsync(order); // Thêm Order trước
                await _uow.SaveChangesAsync(); // Lưu để tránh lỗi FK

                // 3. Tạo SnackOrders sau khi Order đã tồn tại
                if (request.SnackOrders != null)
                {
                    foreach (var item in request.SnackOrders)
                    {
                        var snackOrder = new SnackOrder
                        {
                            OrderId = orderId,
                            SnackId = item.SnackId,
                            Quantity = item.Quantity,
                            SnackOrderEnum = SnackOrderEnum.SNACK
                        };
                        await _uow.SnackOrderRepo.AddAsync(snackOrder);
                    }
                }

                if (request.SnackComboOrders != null)
                {
                    foreach (var item in request.SnackComboOrders)
                    {
                        var snackComboOrder = new SnackOrder
                        {
                            OrderId = orderId,
                            SnackId = item.SnackComboId,
                            Quantity = item.Quantity,
                            SnackOrderEnum = SnackOrderEnum.SNACKCOMBO
                        };
                        await _uow.SnackOrderRepo.AddAsync(snackComboOrder);
                    }
                }

                await _uow.SaveChangesAsync(); // Lưu SnackOrders

                // 4. Load lại Order để ánh xạ
                var savedOrder = await _uow.OrderRepo.GetOrderById(orderId);

                OrderResponse orderResponse = new OrderResponse
                {
                    Id = order.Id,
                    UserId = order.UserId,
                    PaymentMethod = order.PaymentMethod,
                    OrderTime = order.OrderTime,
                    TotalAmount = order.TotalAmount,
                    TotalAfter = order.TotalAmount * discount, // Có thể xử lý khác nếu bạn có khuyến mãi
                    Status = order.Status,
                    SeatSchedules = order.SeatSchedules?.Select(ss => ss.Id).ToList() ?? new List<Guid>(),
                    Snacks = savedOrder.SnackOrders?
                        .Select(s => new SnackOrderResponse
                        {
                            Id = s.Id,
                            OrderId = s.OrderId,
                            SnackId = s.SnackId,
                            Quantity = s.Quantity,
                            SnackOrderEnum = s.SnackOrderEnum
                        }).ToList()
                };
                return apiResp.SetOk(orderResponse);
            }
            catch (Exception ex)
            {
                return apiResp.SetBadRequest(ex.Message);
            }
        }

        public async Task<ApiResp> ViewTicketOrder()
        {
            ApiResp apiResp = new ApiResp();
            try
            {
                var orders = await _uow.OrderRepo.GetAllOrderAsync(
                    o => o.SeatSchedules,
                    o => o.SnackOrders
                );

                var responses = orders.Select(order => new OrderResponse
                {
                    Id = order.Id,
                    UserId = order.UserId,
                    PaymentMethod = order.PaymentMethod,
                    OrderTime = order.OrderTime,
                    TotalAmount = order.TotalAmount,
                    TotalAfter = order.TotalAmount,
                    Status = order.Status,
                    SeatSchedules = order.SeatSchedules?.Select(ss => ss.Id).ToList() ?? new List<Guid>(),
                    Snacks = order.SnackOrders?.Select(s => new SnackOrderResponse
                    {
                        Id = s.Id,
                        OrderId = s.OrderId,
                        SnackId = s.SnackId,
                        Quantity = s.Quantity
                    }).ToList() ?? new List<SnackOrderResponse>()
                }).ToList();

                return apiResp.SetOk(responses);
            }
            catch (Exception ex)
            {
                return apiResp.SetBadRequest(ex);
            }
        }

        public async Task<ApiResp> ViewTicketOrderByUserId(Guid userId)
        {
            ApiResp apiResp = new ApiResp();
            try
            {
                var orders = await _uow.OrderRepo.GetAllAsync(
            o => o.UserId == userId,
            include: query => query
                .Include(o => o.SeatSchedules)
                .Include(o => o.SnackOrders)
        );

                var responses = orders.Select(order => new OrderResponse
                {
                    Id = order.Id,
                    UserId = order.UserId,
                    PaymentMethod = order.PaymentMethod,
                    OrderTime = order.OrderTime,
                    TotalAmount = order.TotalAmount,
                    TotalAfter = order.TotalAmount, // hoặc xử lý giảm giá nếu có
                    Status = order.Status,
                    SeatSchedules = order.SeatSchedules?.Select(ss => ss.Id).ToList() ?? new List<Guid>(),
                    Snacks = order.SnackOrders?.Select(snack => new SnackOrderResponse
                    {
                        Id = snack.Id,
                        OrderId = snack.OrderId,
                        SnackId = snack.SnackId,
                        Quantity = snack.Quantity
                    }).ToList()
                }).ToList();

                if(responses == null)
                {
                    return apiResp.SetNotFound();
                }

                return apiResp.SetOk(responses);
            }
            catch (Exception ex)
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

                foreach (var seatSchedule in seats)
                {
                    seatSchedule.Status = SeatBookingStatus.Available;
                    await _uow.SeatScheduleRepo.UpdateAsync(seatSchedule);
                }
                await _uow.SaveChangesAsync();
                return apiResp.SetOk("Seat changed to Available");

            }
            catch (Exception ex)
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
            catch (Exception ex)
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
