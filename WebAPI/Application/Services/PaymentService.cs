using Application.IServices;
using Application.ViewModel;
using Domain.Enums;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.ViewModel.Response;
using Microsoft.AspNetCore.Http;
using Application.IRepos;
using Domain.Entities;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IVnPayService _vnPayService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthRepo _authRepo;


        public PaymentService(IUnitOfWork uow, IMapper mapper, IVnPayService vnPayService, IHttpContextAccessor httpContextAccessor, IAuthRepo authRepo)
        {
            _uow = uow;
            _mapper = mapper;
            _vnPayService = vnPayService;
            _httpContextAccessor = httpContextAccessor;
            _authRepo = authRepo;
        }


        public async Task<ApiResp> FindPaymentByUserIdAsync(Guid userId)
        {
            try
            {
                
                var payments = await _uow.PaymentRepo.GetAllAsync(
                    p => p.userId == userId && !p.IsDeleted,
                    include: q => q.Include(p => p.Order).Include(p => p.User)
                );

                if (payments == null || !payments.Any())
                {
                    // Test expects ErrorMessage to be null:
                    return new ApiResp().SetNotFound();
                }

                var paymentResponses = _mapper.Map<List<PaymentResponse>>(payments);
                return new ApiResp().SetOk(paymentResponses);
            }
            catch (Exception ex)
            {
                return new ApiResp().SetBadRequest(null, $"Error finding payments: {ex.Message}");
            }
        }
        public async Task<ApiResp> GetAllCashPaymentAsync()
        {
            try
            {
               
                var payments = await _uow.PaymentRepo.GetAllAsync(
                    p => p.PaymentMethod == PaymentMethod.Cash && !p.IsDeleted,
                    include: q => q.Include(p => p.Order).Include(p => p.User)
                );

                if (payments == null || !payments.Any())
                {
                    return new ApiResp().SetNotFound(null, "No cash payments found.");
                }
                var paymentResponses = _mapper.Map<List<PaymentResponse>>(payments);
                return new ApiResp().SetOk(paymentResponses);
               
            }
            catch (Exception ex)
            {
                return new ApiResp().SetBadRequest(null, $"Error retrieving cash payments: {ex.Message}");
            }
        }
        
        public async Task<ApiResp> ChangeStatusFromPendingToSuccessAsync(Guid id)
        {
            try
            {
               
                var payment = await _uow.PaymentRepo.GetByIdAsync(id);
                if (payment == null)
                {
                    return new ApiResp().SetNotFound(null, $"Payment with ID {id} not found.");
                }

                if (payment.Status != PaymentStatus.Pending)
                {
                    return new ApiResp().SetBadRequest(null, $"Payment with ID {id} is not in Pending status.");
                }

                
                payment.Status = PaymentStatus.Success;
                payment.PaymentTime = DateTime.UtcNow;
                payment.UpdateDate = DateTime.UtcNow;

               
                await _uow.PaymentRepo.UpdateAsync(payment);
                
                return new ApiResp().SetOk("Payment status changed from Pending to Success successfully.");
            }
            catch (Exception ex)
            {
                return new ApiResp().SetBadRequest(null, $"Error updating payment status: {ex.Message}");
            }
        }
        public async Task<ApiResp> HandleVnPayReturn(IQueryCollection queryCollection)
        {
            try
            {
                var response = _vnPayService.ProcessResponse(queryCollection);
                using (var transaction = await _uow.BeginTransactionAsync())
                {
                    var orderId = Guid.Parse(response.OrderId);
                    var order = await _uow.OrderRepo.GetByIdAsync(orderId);
                    if (order == null)
                        return new ApiResp().SetNotFound(null, "Order not found.");

                    var payment = await _uow.PaymentRepo.GetAsync(p => p.OrderId == order.Id);
                    if (payment == null)
                        return new ApiResp().SetNotFound(null, "Payment not found.");

                    if (response.Success)
                    {
                        
                        payment.Status = PaymentStatus.Success;
                        payment.PaymentTime = DateTime.UtcNow;
                        
                        if (decimal.TryParse(queryCollection["vnp_Amount"], out var paidAmount))
                        {
                            payment.AmountPaid = paidAmount / 100; 
                        }
                        
                        payment.TransactionCode = queryCollection["vnp_TransactionNo"];
                        await _uow.PaymentRepo.UpdateAsync(payment);

                        order.Status = OrderEnum.Success;
                        await _uow.OrderRepo.UpdateAsync(order);

                        
                        if (order.UserId.HasValue && payment.AmountPaid.HasValue)
                        {
                            try
                            {
                                var user = await _uow.UserRepo.GetByIdAsync(order.UserId.Value);
                                if (user != null)
                                {
                                  
                                    var (domainUser, appUser, roles) = await _authRepo.GetUserWithRolesAndProfileByIdAsync(order.UserId.Value);
                                    
                                    int basePoints = (int)payment.AmountPaid.Value;
                                    int bonusPoints = 0;
                                    
                                    
                                    if (roles.Contains("Member"))
                                    {
                                        bonusPoints = 100; 
                                    }
                                   
                                    
                                    int totalPoints = basePoints + bonusPoints;
                                    user.Score += totalPoints;
                                    
                                    var scoreLog = new ScoreLog
                                    {
                                        UserId = user.Id,
                                        PointsChanged = $"+{totalPoints}",
                                        ActionType = $"Payment Reward{(bonusPoints > 0 ? " (Member Bonus)" : "")}"
                                    };
                                    await _uow.ScoreLogRepo.AddAsync(scoreLog);
                                    await _uow.UserRepo.UpdateAsync(user);
                                }
                                else
                                {
                                    Console.WriteLine($"[ScoreLog Bonus][Warning] User not found for UserId: {order.UserId.Value}");
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"[ScoreLog Bonus][Exception] {ex.Message}\n{ex.StackTrace}");
                                return new ApiResp().SetBadRequest($"Payment succeeded but failed to bonus score: {ex.Message}");
                            }
                        }

                        await _uow.SaveChangesAsync();
                        await transaction.CommitAsync();

                        return new ApiResp().SetOk("Payment successful and order updated.");
                    }

                    return new ApiResp().SetBadRequest(null, "Payment processing failed.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[VNPay][Exception] {ex.Message}");
                return new ApiResp().SetBadRequest(null, $"Error processing VNPay return: {ex.Message}");
            }
        }

        public async Task<ApiResp> HandleVnPayReturnForSubscription(IQueryCollection queryCollection)
        {
            var errors = new List<string>();
            try
            {
               var response = _vnPayService.ProcessResponsee(queryCollection);
               using (var transaction = await _uow.BeginTransactionAsync())
                {
                    var subId = Guid.Parse(response.OrderId);
                    var sub = await _uow.SubscriptionRepo.GetByIdAsync(subId);
                    if (sub == null)
                    {
                        Console.WriteLine($"[VNPay][Return] Subscription not found for subId: {subId}");
                        return new ApiResp().SetNotFound(null, "subscription not found.");
                    }
                    var payment = await _uow.PaymentRepo.GetAsync(p => p.SubscriptionId == subId);
                    if (payment == null)
                    {
                        Console.WriteLine($"[VNPay][Return] Payment not found for subId: {subId}");
                        return new ApiResp().SetNotFound(null, "Payment not found.");
                    }
                    if (response.Success)
                    {
                        payment.Status = PaymentStatus.Success;
                        payment.PaymentTime = DateTime.UtcNow;
                        if (decimal.TryParse(queryCollection["vnp_Amount"], out var paidAmount))
                        {
                            payment.AmountPaid = paidAmount / 100;
                           
                        }
                        payment.TransactionCode = queryCollection["vnp_TransactionNo"];
                        await _uow.PaymentRepo.UpdateAsync(payment);

                        sub.Status = SubscriptionStatus.active;
                        await _uow.SubscriptionRepo.UpdateAsync(sub);
                        if (sub.SubscriptionPlanId != null)
                        {
                            var plan = await _uow.SubscriptionPlanRepo.GetByIdAsync(sub.SubscriptionPlanId.Value);
                            if (plan != null)
                            {
                                plan.Status = PlanStatus.Active;
                                await _uow.SubscriptionPlanRepo.UpdateAsync(plan);
                            }
                        }
                        if (sub.UserId.HasValue)
                        {
                            await _authRepo.RemoveUserFromRoleAsync(sub.UserId.Value, "Customer");
                            await _authRepo.AddUserToRoleAsync(sub.UserId.Value, "Member");
                        }

                        if (sub.UserId.HasValue && payment.AmountPaid.HasValue)
                        {
                            try
                            {
                                var user = await _uow.UserRepo.GetByIdAsync(sub.UserId.Value);
                                if (user != null)
                                {
                                    int points = (int)payment.AmountPaid.Value;
                                    user.Score += points;
                                    var scoreLog = new ScoreLog
                                    {
                                        UserId = user.Id,
                                        PointsChanged = $"+ {points}",
                                        ActionType = "Become Membership"
                                    };
                                    await _uow.ScoreLogRepo.AddAsync(scoreLog);
                                    await _uow.UserRepo.UpdateAsync(user);
                                }
                            }
                            catch (Exception ex)
                            {
                                var errorMsg = $"[ScoreLog Bonus][Exception] {ex.Message}\n{ex.StackTrace}";
                                Console.WriteLine(errorMsg);
                                errors.Add(errorMsg);
                                // Ensure IsSuccess is false for test
                                return new ApiResp().SetBadRequest(null, $"Payment succeeded but failed to bonus score: {ex.Message}");
                            }
                        }

                        await _uow.SaveChangesAsync();
                        await transaction.CommitAsync();
                        return new ApiResp().SetOk("Payment successful and order updated.");
                    }

                    Console.WriteLine("[VNPay][Return] Payment processing failed (response.Success is false).");
                    return new ApiResp().SetBadRequest(null, "Payment processing failed.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[VNPay][Exception] {ex.Message}\n{ex.StackTrace}");
                return new ApiResp().SetBadRequest(null, $"Error processing VNPay return: {ex.Message}");
            }
        }


    }
}

        
