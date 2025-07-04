using Microsoft.AspNetCore.Http;
using Domain.Entities;
using Application.ViewModel.Response;
using Infrastructure.Service;

namespace Application.IServices
{
    public interface IVnPayService
    {
        string CreatePaymentUrl(Order order, HttpContext httpContext);
        VnPaymentResponseModel ProcessResponse(IQueryCollection collections);
        string CreatePaymentUrlForSubscription(Subscription sub, HttpContext context);
        VnPaymentResponseModel ProcessResponsee(IQueryCollection collections);
    }
}