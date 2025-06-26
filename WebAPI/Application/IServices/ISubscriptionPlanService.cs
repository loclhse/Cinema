using Application.ViewModel;
using Application.ViewModel.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IServices
{
    public interface ISubscriptionPlanService
    {
        Task<ApiResp> CreateNewSubscriptionPlanAsync(SubscriptionPlanRequest subscriptionPlanRequest);
        Task<ApiResp> ManagerGetAllSubscriptionPlansAsync();
        Task<ApiResp> GetSubscriptionPlanByIdAsync(Guid id);
        Task<ApiResp> DeleteSubscriptionPlanAsync(Guid id);
        Task<ApiResp> UpdateInActiveSubscriptionPlanAsync(Guid id, SubscriptionPlanRequest subscriptionPlanRequest);
        Task<ApiResp> ActiveSubscriptionPlanAsync(Guid id);
        Task<ApiResp> UserGetAllSubscriptionPlansAsync();
    }
}
