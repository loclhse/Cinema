using Application.ViewModel;
using Application.ViewModel.Request;
using Application.ViewModel.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IServices
{
    public interface IPromotionService
    {
        Task<ApiResp> GetAllPromotion();
        Task<ApiResp> GetPromotionById(Guid id); // Add this missing method signature  
        Task<ApiResp> AddPromotion(EditPromotionRequest editPromotionRequest);
        Task<ApiResp> DeletePromotion(Guid id);
        Task<ApiResp> EditPromotion(Guid id, EditPromotionRequest editPromotionRequest);
    }
}
