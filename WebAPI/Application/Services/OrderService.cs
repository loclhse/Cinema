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
                //lay list snack bang id tu request

                //lay list snackCombo

                //lay list seatSchedules bang id tu request
                List<SeatSchedule> seatSchedules = new();
                foreach (var seatId in request.SeatScheduleId)
                {
                    var seatSchedule = await _uow.SeatScheduleRepo.GetAsync(x => x.Id == seatId);
                    if (seatSchedule != null)
                        seatSchedules.Add(seatSchedule);
                }

                decimal price = await CalculatePriceAsync(request.SeatScheduleId ,request.Snack, request.SnackCombo, request.PromotionId); // gia chua giam tu promotion

                var seatSchedulesMapped = _mapper.Map<List<SeatScheduleForOrderResponse>>(seatSchedules);
                OrderResponse rp = new OrderResponse
                {
                    UserId = request.UserId,
                    OrderTime = DateTime.UtcNow,
                    TotalAmount = price,
                    TotalAfter = price, //can them promotion de chinh lai
                    SeatSchedules = seatSchedulesMapped,
                    Snacks = request.Snack,
                    SnackCombos = request.SnackCombo,
                };

                Order order = _mapper.Map<Order>(rp);
                await _uow.OrderRepo.AddAsync(order);
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

        private async Task<decimal> CalculatePriceAsync(IEnumerable<Guid>? seatSchedules, IEnumerable<SnackOrderRequest>? seatScheduleIds, IEnumerable<SnackComboOrderRequest>? snackIds, Guid? promotion)
        {
            //decimal total = 0m;

            //if (seatScheduleIds?.Any() == true)
            //{
            //    var seatTypePrices = (await _uow.SeatTypePriceRepo.GetAllAsync(null))
            //                         .ToDictionary(x => x.SeatType,
            //                                       x => x.DefaultPrice);

            //    var schedules = await _uow.SeatScheduleRepo.GetAllAsync(
            //                                                    ss => seatScheduleIds.Contains(ss.Id),
            //                                                    include: q => q.Include(ss => ss.Seat!));

            //    foreach (var ss in schedules)
            //    {
            //        if (ss.Seat != null &&
            //            seatTypePrices.TryGetValue(ss.Seat.SeatType, out var price))
            //        {
            //            total += price;
            //        }
            //    }
            //}

            //if (snackIds?.Any() == true)
            //{
            //    var snacks = await _uow.SnackRepo.GetAllAsync(s => snackIds.Contains(s.Id));

            //    total += snacks.Sum(s => s.Price);
            //}

            //return total;
            return 0;
        }

    }
}
