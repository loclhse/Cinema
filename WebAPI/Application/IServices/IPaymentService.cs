using Application.ViewModel;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IServices
{
    public interface IPaymentService
    {
       
        Task<ApiResp> FindPaymentByUserIdAsync(Guid userId);
        Task<ApiResp> GetAllCashPaymentAsync();
        Task<ApiResp> ChangeStatusFromPendingToSuccessAsync(Guid id);
    }
} 