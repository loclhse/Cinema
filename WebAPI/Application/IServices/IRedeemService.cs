using Application.ViewModel;
using Application.ViewModel.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IServices
{
    public interface IRedeemService
    {
        Task<ApiResp> CreateRedeemAsync(Guid userId, List<RedeemRequest> request);
        Task<ApiResp> GetRedeemAsync(Guid redeemId);
        Task<ApiResp> GetAllRedeemsAsync();
        Task<ApiResp> GetPendingRedeemsByAccountAsync(Guid accountId);
        Task<ApiResp> GetPaidRedeemsByAccountAsync(Guid accountId);
        Task<ApiResp> CancelRedeemAsync(Guid id);
        Task<ApiResp> redeemItem(Guid id);
        Task<ApiResp> updateRedeemAsync(Guid redeemId, List<RedeemRequest> requests);
    }
}
