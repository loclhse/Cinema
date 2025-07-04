using Application.IServices;
using Application.ViewModel;
using Application.ViewModel.Request;
using Application.ViewModel.Response;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
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

        public async Task<ApiResp> CancelSubscription(Guid Id)
        {
            var apiResp = new ApiResp();
            try
            {
                var subscription = await _uow.SubscriptionRepo.GetAsync(x => x.Id == Id && !x.IsDeleted);
                if (subscription == null)
                {
                    return apiResp.SetNotFound("Subscription not found");
                }
                subscription.Status = SubscriptionStatus.cancelled;
                await _uow.SaveChangesAsync();
                return apiResp.SetOk("Subscription cancelled !");

            }
            catch (Exception ex)
            {
                return  apiResp.SetBadRequest(ex.Message);
            }
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
                    return apiResp.SetUnauthorized(message:"User not authenticated");
                }
                var sub = _mapper.Map<Subscription>(subscriptionRequest);
                sub.UserId = Guid.Parse(UserId.Value);
                sub.StartDate =  DateOnly.FromDateTime(DateTime.UtcNow);
                sub.EndDate = sub.StartDate.AddDays(plan.Duration);
                sub.Name = plan.Name;
                sub.Price = plan.Price;
                await _uow.SubscriptionRepo.AddAsync(sub);
                await _uow.SaveChangesAsync();
                var payment = new Payment
                {
                    userId = sub.UserId,
                    PaymentMethod = PaymentMethod.VNPay,
                    SubscriptionId = sub.Id,
                    Status = PaymentStatus.Pending,
                    AmountPaid = (decimal)sub.Price,
                };
                await _uow.PaymentRepo.AddAsync(payment);
                await _uow.SaveChangesAsync();
                return apiResp.SetOk("Subscription added successfully!");
            }
            catch (Exception ex)
            {
                return apiResp.SetBadRequest(ex.Message);
            }
        }

        public async Task<ApiResp> DeleteSubscription(Guid Id)
        {
            var apiResp = new ApiResp();
            try {                 
                var subscription = await _uow.SubscriptionRepo.GetAsync(x => x.Id == Id);
                if (subscription == null)
                {
                    return apiResp.SetNotFound("Subscription not found");
                }
                subscription.IsDeleted = true;
                await _uow.SaveChangesAsync();
                return apiResp.SetOk("Subscription deleted successfully!");
            }
            catch (Exception ex)
            {
                return apiResp.SetBadRequest(ex.Message);
            }
        }

        public async Task<ApiResp> GetAllSubscriptions()
        {
            var apiResp = new ApiResp();
            try
            {
                
                var User = _httpContextAccessor.HttpContext.User;
                var UserId = User.FindFirst(ClaimTypes.NameIdentifier);
                if (User == null || UserId == null || !Guid.TryParse(UserId.Value, out Guid userGuid))
                {
                    return apiResp.SetUnauthorized(message:"User not authenticated");
                }
                var subscriptions = await _uow.SubscriptionRepo.GetAllAsync(x => !x.IsDeleted && x.UserId == Guid.Parse(UserId.Value));
                if (subscriptions == null || !subscriptions.Any())
                {
                    return apiResp.SetNotFound("No subscriptions found for this user");
                }
                var result = _mapper.Map<List<SubscriptionResponse>>(subscriptions);
                return apiResp.SetOk(result);
            }
            catch (Exception ex)
            {
                return apiResp.SetBadRequest(ex.Message);
            }
        }

        public async Task<ApiResp> GetSubscriptionById(Guid Id)
        {
            var apiResp = new ApiResp();
            try { 
                var subscription = await _uow.SubscriptionRepo.GetAsync(x => x.Id == Id && !x.IsDeleted);
                if (subscription == null)
                {
                    return apiResp.SetNotFound("Subscription not found");
                }
                var result = _mapper.Map<SubscriptionResponse>(subscription);
                return apiResp.SetOk(result);
            }
            catch (Exception ex)
            {
                return apiResp.SetBadRequest(ex.Message);
            }
        }

        public async Task<ApiResp> UpdateSubscription(Guid Id, SubscriptionRequest subscriptionRequest)
        {
            var apiResp = new ApiResp();
            try
            {
               var sub = await _uow.SubscriptionRepo.GetAsync(x => x.Id == Id && !x.IsDeleted);
                if (sub == null)
                {
                    return apiResp.SetNotFound("Subscription not found");
                }
                var plan = await _uow.SubscriptionPlanRepo.GetByIdAsync(subscriptionRequest.SubscriptionPlanId);
                if (plan == null)
                {
                    return apiResp.SetNotFound("Subscription plan not found");
                }
                sub.SubscriptionPlanId = subscriptionRequest.SubscriptionPlanId;
                sub.StartDate = DateOnly.FromDateTime(DateTime.UtcNow);
                sub.EndDate = sub.StartDate.AddDays(plan.Duration);
                
                await _uow.SaveChangesAsync();
                return apiResp.SetOk("Subscription updated successfully!");
            }
            catch (Exception ex)
            {
                return apiResp.SetBadRequest(ex.Message);
            }
        }
        
       
    }
}
