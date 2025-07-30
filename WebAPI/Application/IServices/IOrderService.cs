using Application.ViewModel;
using Application.ViewModel.Request;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IServices
{
    public interface IOrderService
    {
        Task<ApiResp> CreateTicketOrder(OrderRequest request);
        Task<ApiResp> ViewTicketOrder();
        Task<ApiResp> ViewTicketOrderByUserId(Guid userId);
        Task<ApiResp> CancelTicketOrderById(List<Guid> seatScheduleId, Guid orderId);
        Task<ApiResp> SuccessOrder(List<Guid> seatScheduleId, Guid orderId, Guid userId, Guid? movieId);
    }
}
