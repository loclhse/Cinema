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
        Task<ApiResp> CreateRedeemAsync(RedeemRequest request);
        Task<ApiResp> GetRedeemAsync(Guid redeemId);
        Task<ApiResp> GetAllRedeemsAsync();
        Task<ApiResp> GetRedeemsByAccountAsync(Guid accountId);
        Task<ApiResp> CancelRedeemAsync(Guid id);
    }
}
