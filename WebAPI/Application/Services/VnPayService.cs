using Application.IServices;
using Application.ViewModel.Response;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Infrastructure.Service
{
    public class VnPayService : IVnPayService
    {
        private readonly string _vnp_Url = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
        private readonly string _vnp_Returnurl = "https://localhost:7093/api/Payment/vnpay-return"; // Change to your return URL
        private readonly string _vnp_TmnCode = "FMKBQURC";
        private readonly string _vnp_HashSecret = "ID4X6DY3MQBGY6972GYCKP7SU9YUX0WV";

        public string CreatePaymentUrl(Order order, HttpContext httpContext)
        {
            var vnp_Params = new SortedDictionary<string, string>
    {
        { "vnp_Version", "2.1.0" },
        { "vnp_Command", "pay" },
        { "vnp_TmnCode", _vnp_TmnCode },
        { "vnp_Amount", ((int)(order.TotalAmount * 100)).ToString() },
        { "vnp_CurrCode", "VND" },
        { "vnp_TxnRef", order.Id.ToString() },
        { "vnp_OrderInfo", $"Payment for order {order.Id}" },
        { "vnp_OrderType", "other" },
        { "vnp_Locale", "vn" },
        { "vnp_ReturnUrl", _vnp_Returnurl },
        { "vnp_IpAddr", "127.0.0.1" },
        { "vnp_CreateDate", DateTime.UtcNow.ToString("yyyyMMddHHmmss") }
    };

            var hashData = string.Join("&", vnp_Params
                .Where(kv => !string.IsNullOrEmpty(kv.Value) && kv.Key != "vnp_SecureHash" && kv.Key != "vnp_SecureHashType")
                .OrderBy(kv => kv.Key)
                .Select(kv => kv.Key + "=" + Uri.EscapeDataString(kv.Value.Trim())));
            Console.WriteLine("Request hashData: [" + hashData + "]");

            string secureHash = GetHmacSha512(_vnp_HashSecret, hashData);
            Console.WriteLine("Request secureHash: " + secureHash);

            var query = hashData + "&vnp_SecureHash=" + secureHash;
            return _vnp_Url + "?" + query;
        }

        private string GetHmacSha512(string key, string data)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var dataBytes = Encoding.UTF8.GetBytes(data);
            using (var hmac = new HMACSHA512(keyBytes))
            {
                var hashBytes = hmac.ComputeHash(dataBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }

        public VnPayResponseModel ProcessResponse(IQueryCollection queryCollection)
        {
            var vnpData = queryCollection.Where(kvp => kvp.Key.StartsWith("vnp_"))
                                        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToString(), StringComparer.Ordinal);
            Console.WriteLine("Callback vnpData: " + string.Join(", ", vnpData.Select(kvp => $"{kvp.Key}={kvp.Value}")));

            var signedFields = vnpData.Where(kvp => kvp.Key != "vnp_SecureHash" && kvp.Key != "vnp_SecureHashType")
                                     .OrderBy(kvp => kvp.Key, StringComparer.Ordinal)
                                     .ToList();

            var hashData = string.Join("&", signedFields.Select(kvp => kvp.Key + "=" + Uri.EscapeDataString(kvp.Value.Trim())));
            Console.WriteLine("[Callback hashData: [" + hashData + "]]");
            var computedHash = GetHmacSha512(_vnp_HashSecret, hashData);
            Console.WriteLine($"Computed Hash: {computedHash}, Hash Data: {hashData}");

            string vnpSecureHash = vnpData.ContainsKey("vnp_SecureHash") ? vnpData["vnp_SecureHash"] : "";
            Console.WriteLine($"VNPay Hash: {vnpSecureHash}");

            bool isValidSignature = !string.IsNullOrEmpty(vnpSecureHash) && computedHash.Equals(vnpSecureHash, StringComparison.OrdinalIgnoreCase);

            string responseCode = vnpData.ContainsKey("vnp_ResponseCode") ? vnpData["vnp_ResponseCode"] : "";
            string orderId = vnpData.ContainsKey("vnp_TxnRef") ? vnpData["vnp_TxnRef"] : "";

            return new VnPayResponseModel
            {
                IsSuccess = isValidSignature && responseCode == "00",
                OrderId = orderId,
                TransactionStatus = responseCode,
                Message = isValidSignature
                    ? (responseCode == "00" ? "Payment successful" : "Payment failed")
                    : "Invalid VNPay signature"
            };
        }
    }
}