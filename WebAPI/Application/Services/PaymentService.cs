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

namespace Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public PaymentService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
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
                    return new ApiResp().SetNotFound("No payments found for this user.");
                }

                var paymentResponses = _mapper.Map<List<PaymentResponse>>(payments);
                return new ApiResp().SetOk(paymentResponses);
            }
            catch (Exception ex)
            {
                return new ApiResp().SetBadRequest($"Error finding payments: {ex.Message}");
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
                    return new ApiResp().SetNotFound("No cash payments found.");
                }
                var paymentResponses = _mapper.Map<List<PaymentResponse>>(payments);
                return new ApiResp().SetOk(paymentResponses);
               
            }
            catch (Exception ex)
            {
                return new ApiResp().SetBadRequest($"Error retrieving cash payments: {ex.Message}");
            }
        }
        
        public async Task<ApiResp> ChangeStatusFromPendingToSuccessAsync(Guid id)
        {
            try
            {
                // Using GenericRepo's GetByIdAsync
                var payment = await _uow.PaymentRepo.GetByIdAsync(id);
                if (payment == null)
                {
                    return new ApiResp().SetNotFound($"Payment with ID {id} not found.");
                }

                if (payment.Status != PaymentStatus.Pending)
                {
                    return new ApiResp().SetBadRequest($"Payment with ID {id} is not in Pending status.");
                }

                // Update the payment status
                payment.Status = PaymentStatus.Success;
                payment.PaymentTime = DateTime.UtcNow;
                payment.UpdateDate = DateTime.UtcNow;

                // Using GenericRepo's UpdateAsync
                await _uow.PaymentRepo.UpdateAsync(payment);
                
                return new ApiResp().SetOk("Payment status changed from Pending to Success successfully.");
            }
            catch (Exception ex)
            {
                return new ApiResp().SetBadRequest($"Error updating payment status: {ex.Message}");
            }
        }
    }
}

        
