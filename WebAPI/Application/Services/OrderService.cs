using Application.IServices;
using Application.ViewModel;
using Application.ViewModel.Request;
using Application.ViewModel.Response;
using AutoMapper;
using Domain.Entities;
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
                List<Snack> snacks = new();
                foreach (var snackId in request.SnackId)
                {
                    var snack = await _uow.SnackRepo.GetAsync(x => x.Id == snackId);
                    if (snack != null)
                        snacks.Add(snack);
                }

                //lay list seatSchedules bang id tu request
                List<SeatSchedule> seatSchedules = new();
                foreach (var seatId in request.SeatScheduleId)
                {
                    var seatSchedule = await _uow.SeatScheduleRepo.GetAsync(x => x.Id == seatId);
                    if (seatSchedule != null)
                        seatSchedules.Add(seatSchedule);
                }

                double price = CalculatePrice(request.SeatScheduleId, request.SnackId);

                var seatSchedulesMapped = _mapper.Map<List<SeatScheduleForOrderResponse>>(seatSchedules);
                OrderResponse rp = new OrderResponse
                {
                    UserId = request.UserId,
                    OrderTime = DateTime.UtcNow,
                    TotalAmount = 12, //dang fix cung, sua lai theo CalculatePrice
                    TotalAfter = 12, //dang fix cung, sua lai theo CalculatePrice
                    SeatSchedules = seatSchedulesMapped,
                    Snacks = snacks,
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

        private double CalculatePrice(List<Guid>? SeatScheduleId, List<Guid>? SnackId)
        {
            return 0;
        }
    }
}
