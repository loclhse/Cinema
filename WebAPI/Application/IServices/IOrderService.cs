using Application.ViewModel;
using Application.ViewModel.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IServices
{
    public interface IOrderService
    {
        Task<ApiResp> CreateTicketOrder(OrderRequest request);
    }
}
