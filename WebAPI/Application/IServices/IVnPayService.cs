using Microsoft.AspNetCore.Http;
using Domain.Entities;
using Application.ViewModel.Response;
using Infrastructure.Service;
using Application.ViewModel;

namespace Application.IServices
{
    public interface IVnPayService
    {
        ApiResp CreatePaymentUrl(Order order, HttpContext httpContext);
        VnPaymentResponseModel ProcessResponse(IQueryCollection collections);
        ApiResp CreatePaymentUrlForSubscription(Subscription sub, HttpContext context);
        VnPaymentResponseModel ProcessResponsee(IQueryCollection collections);
    }
}