using Application.IServices;
using Application.ViewModel;
using Application.ViewModel.Request;
using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class PromotionService : IPromotionService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public PromotionService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<ApiResp> AddPromotion(EditPromotionRequest editPromotionRequest)
        {
            ApiResp apiResp = new ApiResp();
            try
            {
                var rs = _mapper.Map<Promotion>(editPromotionRequest);
                rs.Id = Guid.NewGuid();
                await _uow.PromotionRepo.AddAsync(rs);
                var save = await _uow.SaveChangesAsync();
                if (save > 0)
                {
                    return apiResp.SetOk("Successfully updated");
                }
                else
                {
                    return apiResp.SetBadRequest();
                }
            }catch(Exception ex)
            {
                return apiResp.SetBadRequest(null, ex.Message);
            }
        }

        public async Task<ApiResp> DeletePromotion(Guid id)
        {
            ApiResp apiResp = new();
            try
            {
                var promo = await _uow.PromotionRepo.GetPromotionById(id);
                if(promo == null)
                {
                    return apiResp.SetNotFound(null, "Not found this Promotion");
                }
                promo.IsDeleted = true;
                if (await _uow.SaveChangesAsync() > 0)
                {
                    return apiResp.SetOk("Delete Successfully");
                }
                return apiResp.SetBadRequest(null, "Deleted failed, could not save");

            }catch(Exception ex)
            {
                return apiResp.SetBadRequest(null, ex.Message);
            }
        }

        public async Task<ApiResp> EditPromotion(Guid id, EditPromotionRequest editPromotionRequest)
        {
            ApiResp apiResp= new();
            try
            {
                var promo = await _uow.PromotionRepo.GetPromotionById(id);
                if (promo == null)
                {
                    return apiResp.SetNotFound(null, "Promotion not found");
                }
                var rs = _mapper.Map(editPromotionRequest, promo);
                await _uow.SaveChangesAsync();
                return apiResp.SetOk(rs);
            }catch(Exception ex)
            {
                return apiResp.SetBadRequest(null, ex.Message) ;
            }
        }

        public async Task<ApiResp> GetAllPromotion()
        {
            ApiResp rp = new ApiResp();
            try
            {
                var promo = await _uow.PromotionRepo.GetAllPromotion();
                if(!promo.Any())
                {
                    return rp.SetNotFound(null, "Not found any Promotion");
                }
                return rp.SetOk(promo);
            }catch(Exception ex)
            {
                return rp.SetBadRequest(null, ex.Message) ;
            }
        }

        public async Task<ApiResp> GetPromotionById(Guid id)
        {
            ApiResp apiResp = new ApiResp();
            try
            {
                var promo = await _uow.PromotionRepo.GetPromotionById(id);
                if(promo == null)
                {
                    return apiResp.SetNotFound(null, "Not found this Promotion");
                }
                return apiResp.SetOk(promo);
            }catch(Exception e)
            {
                return apiResp.SetBadRequest(null, e.Message) ;
            }
        }
    }
}
