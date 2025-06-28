using Application.ViewModel;
using Application.ViewModel.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IServices
{
    public interface ISubscriptionService
    {
        Task<ApiResp> CreateSubscription(SubscriptionRequest subscriptionRequest);
        Task<ApiResp> GetSubscriptionById(Guid Id);
        Task<ApiResp> GetAllSubscriptions();
        Task<ApiResp> UpdateSubscription(Guid Id, SubscriptionRequest subscriptionRequest);
        Task<ApiResp> DeleteSubscription(Guid Id);
    }
}
