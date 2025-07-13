using Application.IServices;
using Application.ViewModel.Response;
using Domain.Entities;
using Infrastructure.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Application.ViewModel;


namespace WebAPI.Infrastructure.Services
{
   
   

   

    public class VnPayService : IVnPayService
    {
        private readonly IConfiguration _configuration;


        public VnPayService(IConfiguration config)
        {
            _configuration = config ?? throw new ArgumentNullException(nameof(config));
        }



        public ApiResp CreatePaymentUrl(Order order, HttpContext context)
        {
            var tick = DateTime.Now.Ticks.ToString();

            var vnpay = new VnPayLibrary();
            vnpay.AddRequestData("vnp_Version", _configuration["VnPay:Version"]);
            vnpay.AddRequestData("vnp_Command", _configuration["VnPay:Command"]);
            vnpay.AddRequestData("vnp_TmnCode", _configuration["VnPay:TmnCode"]);
            vnpay.AddRequestData("vnp_Amount", ((long)order.TotalAmount * 100000).ToString());
            vnpay.AddRequestData("vnp_CreateDate", DateTime.UtcNow.ToString("yyyyMMddHHmmss")); // Fix: "yyyyMMddHHmmss"
            vnpay.AddRequestData("vnp_CurrCode", _configuration["VnPay:CurrCode"]);
            vnpay.AddRequestData("vnp_IpAddr", Utils.GetIpAddress(context));
            vnpay.AddRequestData("vnp_Locale", _configuration["VnPay:Locale"]);
            vnpay.AddRequestData("vnp_OrderInfo", "Thanh toán cho đơn hàng:" + order.Id);
            vnpay.AddRequestData("vnp_OrderType", "other");
            vnpay.AddRequestData("vnp_ReturnUrl", _configuration["VnPay:PaymentBackReturnUrl"]);
            vnpay.AddRequestData("vnp_TxnRef", order.Id.ToString());
            // Remove empty values and exclude secure hash fields

            var paymentUrl = vnpay.CreateRequestUrl(_configuration["VnPay:PayUrl"], _configuration["VnPay:HashSecret"]);
            return new ApiResp().SetOk(new { PaymentUrl = paymentUrl });
        }

        public ApiResp CreatePaymentUrlForSubscription(Subscription sub, HttpContext context)
        {
            var tick = DateTime.Now.Ticks.ToString();

            var vnpay = new VnPayLibrary();
            vnpay.AddRequestData("vnp_Version", _configuration["VnPay:Version"]);
            vnpay.AddRequestData("vnp_Command", _configuration["VnPay:Command"]);
            vnpay.AddRequestData("vnp_TmnCode", _configuration["VnPay:TmnCode"]);
            vnpay.AddRequestData("vnp_Amount", ((long)sub.Price * 100000).ToString());
            vnpay.AddRequestData("vnp_CreateDate", DateTime.UtcNow.ToString("yyyyMMddHHmmss")); // Fix: "yyyyMMddHHmmss"
            vnpay.AddRequestData("vnp_CurrCode", _configuration["VnPay:CurrCode"]);
            vnpay.AddRequestData("vnp_IpAddr", Utils.GetIpAddress(context));
            vnpay.AddRequestData("vnp_Locale", _configuration["VnPay:Locale"]);
            vnpay.AddRequestData("vnp_OrderInfo", "Thanh toán cho đơn hàng:" + sub.Id);
            vnpay.AddRequestData("vnp_OrderType", "other");
            vnpay.AddRequestData("vnp_ReturnUrl", _configuration["VnPay:PaymentBackReturnUrll"]);
            vnpay.AddRequestData("vnp_TxnRef", sub.Id.ToString());
            // Remove empty values and exclude secure hash fields

            var paymentUrl = vnpay.CreateRequestUrl(_configuration["VnPay:PayUrl"], _configuration["VnPay:HashSecret"]);
            return new ApiResp().SetOk(new { PaymentUrl = paymentUrl });
        }



        public VnPaymentResponseModel ProcessResponse(IQueryCollection collections)
            {
                var vnpay = new VnPayLibrary();
            foreach (var (key, value) in collections)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                    vnpay.AddResponseData(key, value.ToString());
            }
                    var vnp_orderId = vnpay.GetResponseData("vnp_TxnRef");
                    var vnp_TransactionId = Convert.ToInt64(vnpay.GetResponseData("vnp_TransactionNo"));
                    var vnp_SecureHash = collections.FirstOrDefault(p => p.Key == "vnp_SecureHash").Value;
                    var vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
                    var vnp_OrderInfo = vnpay.GetResponseData("vnp_OrderInfo");
            bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, _configuration["VnPay:HashSecret"]);
         if(!checkSignature)
         {
                    return new VnPaymentResponseModel
                    {
                        Success = false
                    };
                }

                return new VnPaymentResponseModel
                {
                    Success = true,
                    OrderId = vnp_orderId.ToString(),
                    TransactionId = vnp_TransactionId.ToString(),
                    

                };
            }

        public VnPaymentResponseModel ProcessResponsee(IQueryCollection collections)
        {
            var vnpay = new VnPayLibrary();
            foreach (var (key, value) in collections)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                    vnpay.AddResponseData(key, value.ToString());
            }
            var vnp_orderId = vnpay.GetResponseData("vnp_TxnRef");
            var vnp_TransactionId = Convert.ToInt64(vnpay.GetResponseData("vnp_TransactionNo"));
            var vnp_SecureHash = collections.FirstOrDefault(p => p.Key == "vnp_SecureHash").Value;
            var vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
            var vnp_OrderInfo = vnpay.GetResponseData("vnp_OrderInfo");
            bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, _configuration["VnPay:HashSecret"]);
            if (!checkSignature)
            {
                return new VnPaymentResponseModel
                {
                    Success = false
                };
            }

            return new VnPaymentResponseModel
            {
                Success = true,
                OrderId = vnp_orderId.ToString(),
                TransactionId = vnp_TransactionId.ToString(),


            };
        }


    }
}