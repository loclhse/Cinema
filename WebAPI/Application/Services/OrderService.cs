using Application.IServices;
using Application.ViewModel;
using Application.ViewModel.Request;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    internal class OrderService : IOrderService
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
                double price = CalculatePrice(request.SeatScheduleId);
                if(request.UserId != null)
                {
                    int score = (int)price;
                }
                return apiResp;
            }catch (Exception ex)
            {
                return apiResp.SetBadRequest(ex.Message);
            }
        }

        private double CalculatePrice(List<Guid>? SeatScheduleId)
        {
            return 0;
        }

        private string Payment(double price)
        {
            return string.Empty;
        }
    }
}
