using Application.IServices;
using Application.ViewModel;
using Application.ViewModel.Request;
using AutoMapper;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        public SubscriptionService(IHttpContextAccessor httpContextAccessor, IMapper mapper, IUnitOfWork unitOfWork)
        {
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
            _uow = unitOfWork;
        }
        public async Task<ApiResp> CreateSubscription(SubscriptionRequest subscriptionRequest)
        {
            ApiResp apiResp = new ApiResp();
            try
            {
                var plan = await _uow.SubscriptionPlanRepo.GetByIdAsync(subscriptionRequest.SubscriptionPlanId);
                var User = _httpContextAccessor.HttpContext.User;
                var UserId = User.FindFirst(ClaimTypes.NameIdentifier);
                if(UserId == null)
                {
                    return apiResp.SetUnauthorized("User not authenticated");
                }
                var sub = _mapper.Map<Subscription>(subscriptionRequest);
                sub.UserId = Guid.Parse(UserId.Value);
                sub.StartDate = DateTime.UtcNow;
                sub.EndDate = sub.StartDate.Value.AddDays(plan.Duration);
                await _uow.SubscriptionRepo.AddAsync(sub);
                await _uow.SaveChangesAsync();
                return apiResp.SetOk("Subscription added successfully!");
            }
            catch (Exception ex)
            {
                return apiResp.SetBadRequest(ex.Message);
            }
        }

        public Task<ApiResp> DeleteSubscription(Guid Id)
        {
            throw new NotImplementedException();
        }

        public Task<ApiResp> GetAllSubscriptions()
        {
            throw new NotImplementedException();
        }

        public Task<ApiResp> GetSubscriptionById(Guid Id)
        {
            throw new NotImplementedException();
        }

        public Task<ApiResp> UpdateSubscription(Guid Id, SubscriptionRequest subscriptionRequest)
        {
            throw new NotImplementedException();
        }
    }
}
