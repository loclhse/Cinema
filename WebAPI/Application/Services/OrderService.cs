using Application.IServices;
using Application.ViewModel;
using Application.ViewModel.Request;
using Application.ViewModel.Response;
using AutoMapper;
using Domain.Entities;
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
                };
                Order order = _mapper.Map<Order>(rp);
                await _uow.OrderRepo.AddAsync(order);

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

    }
}
