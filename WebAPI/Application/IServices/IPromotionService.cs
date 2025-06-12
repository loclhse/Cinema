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
        public Task<ApiResp> GetAllPromotion();

        public Task<ApiResp> GetPromotionById(Guid id);

        public Task<ApiResp> AddPromotion(EditPromotionRequest editPromotionRequest);

        public Task<ApiResp> DeletePromotion(Guid id);

        public Task<ApiResp> EditPromotion(Guid id, EditPromotionRequest editPromotionRequest);
    }
}
