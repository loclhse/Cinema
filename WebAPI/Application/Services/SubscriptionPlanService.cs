using Application.IServices;
using Application.ViewModel;
using Application.ViewModel.Request;
using Application.ViewModel.Response;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class SubscriptionPlanService : ISubscriptionPlanService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public SubscriptionPlanService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<ApiResp> ActiveSubscriptionPlanAsync(Guid id)
        {
            var apiResp = new ApiResp(); 
            try
            {
                var subPlan = await _unitOfWork.SubscriptionPlanRepo.GetByIdAsync(id);
                if (subPlan == null || subPlan.IsDeleted)
                {
                    return apiResp.SetNotFound( "Subscription plan not found");
                }
                subPlan.Status = PlanStatus.Active; 
                await _unitOfWork.SaveChangesAsync();
                return apiResp.SetOk("Subscription plan activated successfully");
            }
            catch(Exception ex)
            {
                return apiResp.SetBadRequest(ex.Message);
            }

        }

        public async Task<ApiResp> CreateNewSubscriptionPlanAsync(SubscriptionPlanRequest subscriptionPlanRequest)
        {
            var apiResp = new ApiResp();
            try
            {
                var subscriptionPlan = _mapper.Map<SubscriptionPlan>(subscriptionPlanRequest);
                if (subscriptionPlan == null)
                {
                    return apiResp.SetBadRequest("Invalid subscription plan data");
                }
                await _unitOfWork.SubscriptionPlanRepo.AddAsync(subscriptionPlan);
                await _unitOfWork.SaveChangesAsync();
                return apiResp.SetOk($"Subscription plan created successfully ID:{subscriptionPlan.Id}");
            }
            catch (Exception ex)
            {
                return apiResp.SetBadRequest(ex.Message);
            }
        }

        public async Task<ApiResp> DeleteSubscriptionPlanAsync(Guid id)
        {
            var apiResp = new ApiResp();
            try
            {
                var subPlan = await _unitOfWork.SubscriptionPlanRepo.GetByIdAsync(id);
                if (subPlan == null || subPlan.IsDeleted)
                {
                    return apiResp.SetNotFound("Subscription plan not found");
                }
                subPlan.IsDeleted = true; 
                await _unitOfWork.SaveChangesAsync();
                return apiResp.SetOk("Subscription plan deleted successfully");
            }
            catch (Exception ex)
            {
                return apiResp.SetBadRequest(ex.Message);
            }
        }

        public async Task<ApiResp> ManagerGetAllSubscriptionPlansAsync()
        {
            var apiResp = new ApiResp();
            try
            {
                var subscriptionPlans = await _unitOfWork.SubscriptionPlanRepo.GetAllAsync(x => !x.IsDeleted);
                if (subscriptionPlans == null || !subscriptionPlans.Any())
                {
                    return apiResp.SetNotFound("No subscription plans found");
                }
                var result = _mapper.Map<List<SubscriptionPlanResponse>>(subscriptionPlans);
                return apiResp.SetOk(result);
            }
            catch (Exception ex)
            {
                return apiResp.SetBadRequest(ex.Message);
            }
        }

        public async Task<ApiResp> GetSubscriptionPlanByIdAsync(Guid id)
        {
            var apiResp = new ApiResp();
            try
            {
                var subPlan = await _unitOfWork.SubscriptionPlanRepo.GetByIdAsync(id);
                if (subPlan == null || subPlan.IsDeleted)
                {
                    return apiResp.SetNotFound("Subscription plan not found");
                }
                var result = _mapper.Map<SubscriptionPlanResponse>(subPlan);
                return apiResp.SetOk(result);
            }
            catch (Exception ex)
            {
                return apiResp.SetBadRequest(ex.Message);
            }
        }

        public async Task<ApiResp> UpdateInActiveSubscriptionPlanAsync(Guid id, SubscriptionPlanRequest subscriptionPlanRequest)
        {
            var apiResp = new ApiResp();
            try
            {
                var subPlan = await _unitOfWork.SubscriptionPlanRepo.GetByIdAsync(id);
                if (subPlan == null || subPlan.IsDeleted)
                {
                    return apiResp.SetNotFound("Subscription plan not found");
                }
                else if (subPlan.Status == PlanStatus.Active)
                {
                    return apiResp.SetBadRequest("Cannot update an active subscription plan.");
                }
                _mapper.Map(subscriptionPlanRequest, subPlan);
                await _unitOfWork.SaveChangesAsync();
                return apiResp.SetOk("Subscription plan updated successfully");
            }
            catch (Exception ex)
            {
                return apiResp.SetBadRequest(ex.Message);
            }
        }

        public async Task<ApiResp> UserGetAllSubscriptionPlansAsync()
        {
            var apiResp = new ApiResp();
            try
            {
                var subscriptionPlans = await _unitOfWork.SubscriptionPlanRepo.GetAllAsync(x => !x.IsDeleted && x.Status == PlanStatus.Active);
                if (subscriptionPlans == null || !subscriptionPlans.Any())
                {
                    return apiResp.SetNotFound("No subscription plans found");
                }
                var result = _mapper.Map<List<SubscriptionPlanResponse>>(subscriptionPlans);
                return apiResp.SetOk(result);
            }
            catch (Exception ex)
            {
                return apiResp.SetBadRequest(ex.Message);
            }
        }
    }
}
