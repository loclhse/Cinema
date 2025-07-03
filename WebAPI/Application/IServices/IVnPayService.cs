using Microsoft.AspNetCore.Http;
using Domain.Entities;
using Application.ViewModel.Response;

namespace Application.IServices
{
    public interface IVnPayService
    {
        string CreatePaymentUrl(Order order, HttpContext httpContext);
        VnPayResponseModel ProcessResponse(IQueryCollection queryCollection);
    }
}