using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Application.ViewModel;
using Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;
using WebAPI.Infrastructure.Services;
using Xunit;
using Microsoft.Extensions.Primitives;

namespace ZTest.Services
{
    public class VnPayServiceTests
    {
        private readonly Mock<IConfiguration> _configMock = new();
        private readonly VnPayService _service;

        public VnPayServiceTests()
        {
            _configMock.Setup(c => c["VnPay:Version"]).Returns("2.1.0");
            _configMock.Setup(c => c["VnPay:Command"]).Returns("pay");
            _configMock.Setup(c => c["VnPay:TmnCode"]).Returns("TESTCODE");
            _configMock.Setup(c => c["VnPay:CurrCode"]).Returns("VND");
            _configMock.Setup(c => c["VnPay:Locale"]).Returns("vn");
            _configMock.Setup(c => c["VnPay:PayUrl"]).Returns("https://pay.vnpay.vn");
            _configMock.Setup(c => c["VnPay:HashSecret"]).Returns("secret");
            _configMock.Setup(c => c["VnPay:PaymentBackReturnUrl"]).Returns("https://return.url");
            _configMock.Setup(c => c["VnPay:PaymentBackReturnUrll"]).Returns("https://return.url");
            _service = new VnPayService(_configMock.Object);
        }

        [Fact]
        public void CreatePaymentUrl_Should_Return_Url()
        {
            var order = new Order { Id = Guid.NewGuid(), TotalAmount = 100 };
            var context = new DefaultHttpContext();
            var resp = _service.CreatePaymentUrl(order, context);
            resp.IsSuccess.Should().BeTrue();
            resp.Result.Should().NotBeNull();
            resp.Result.ToString().Should().Contain("vnp_TmnCode=TESTCODE");
        }

        [Fact]
        public void CreatePaymentUrlForSubscription_Should_Return_Url()
        {
            var sub = new Subscription { Id = Guid.NewGuid(), Price = 200 };
            var context = new DefaultHttpContext();
            var resp = _service.CreatePaymentUrlForSubscription(sub, context);
            resp.IsSuccess.Should().BeTrue();
            resp.Result.Should().NotBeNull();
            resp.Result.ToString().Should().Contain("vnp_TmnCode=TESTCODE");
        }

        // [Fact]
        // public void ProcessResponse_Should_Return_Success_When_Valid_Signature()
        // {
        //     var rawData = "vnp_OrderInfo=info&vnp_ResponseCode=00&vnp_TxnRef=ORDER123&vnp_TransactionNo=456";
        //     var hash = ComputeVnpayHash("dummyhash", rawData);
        //     var query = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
        //     {
        //         { "vnp_TxnRef", "ORDER123" },
        //         { "vnp_TransactionNo", "456" },
        //         { "vnp_SecureHash", hash },
        //         { "vnp_ResponseCode", "00" },
        //         { "vnp_OrderInfo", "info" }
        //     });
        //     var service = new VnPayService(new TestVnPayConfigValidSignature());
        //     var result = service.ProcessResponse(query);
        //     result.Success.Should().BeTrue();
        //     result.OrderId.Should().Be("ORDER123");
        //     result.TransactionId.Should().Be("456");
        // }

        [Fact]
        public void ProcessResponse_Should_Return_Failure_When_Invalid_Signature()
        {
            var query = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
                { "vnp_TxnRef", "ORDER123" },
                { "vnp_TransactionNo", "456" },
                { "vnp_SecureHash", "invalid" },
                { "vnp_ResponseCode", "00" },
                { "vnp_OrderInfo", "info" }
            });
            var service = new VnPayService(new TestVnPayConfigInvalidSignature());
            var result = service.ProcessResponse(query);
            result.Success.Should().BeFalse();
        }

        // [Fact]
        // public void ProcessResponsee_Should_Return_Success_When_Valid_Signature()
        // {
        //     var rawData = "vnp_OrderInfo=info&vnp_ResponseCode=00&vnp_TxnRef=ORDER999&vnp_TransactionNo=789";
        //     var hash = ComputeVnpayHash("dummyhash", rawData);
        //     var query = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
        //     {
        //         { "vnp_TxnRef", "ORDER999" },
        //         { "vnp_TransactionNo", "789" },
        //         { "vnp_SecureHash", hash },
        //         { "vnp_ResponseCode", "00" },
        //         { "vnp_OrderInfo", "info" }
        //     });
        //     var service = new VnPayService(new TestVnPayConfigValidSignature());
        //     var result = service.ProcessResponsee(query);
        //     result.Success.Should().BeTrue();
        //     result.OrderId.Should().Be("ORDER999");
        //     result.TransactionId.Should().Be("789");
        // }

        [Fact]
        public void ProcessResponsee_Should_Return_Failure_When_Invalid_Signature()
        {
            var query = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
                { "vnp_TxnRef", "ORDER999" },
                { "vnp_TransactionNo", "789" },
                { "vnp_SecureHash", "invalid" },
                { "vnp_ResponseCode", "00" },
                { "vnp_OrderInfo", "info" }
            });
            var service = new VnPayService(new TestVnPayConfigInvalidSignature());
            var result = service.ProcessResponsee(query);
            result.Success.Should().BeFalse();
        }

        [Fact]
        public void ProcessResponse_Should_Handle_Empty_SecureHash()
        {
            var query = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
                { "vnp_TxnRef", "ORDER123" },
                { "vnp_TransactionNo", "456" },
                { "vnp_SecureHash", "" },
                { "vnp_ResponseCode", "00" },
                { "vnp_OrderInfo", "info" }
            });
            var service = new VnPayService(new TestVnPayConfigValidSignature());
            var result = service.ProcessResponse(query);
            result.Success.Should().BeFalse();
        }

        private string ComputeVnpayHash(string secret, string rawData)
        {
            using var hmac = new System.Security.Cryptography.HMACSHA512(System.Text.Encoding.UTF8.GetBytes(secret));
            var hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(rawData));
            return string.Concat(hash.Select(b => b.ToString("x2")));
        }

        // Helper config mocks for signature validation
        private class TestVnPayConfigValidSignature : IConfiguration
        {
            public string this[string key]
            {
                get
                {
                    if (key == "VnPay:HashSecret") return "dummyhash";
                    return "test";
                }
                set => throw new NotImplementedException();
            }
            public IEnumerable<IConfigurationSection> GetChildren() => throw new NotImplementedException();
            public IChangeToken GetReloadToken() => null;
            public IConfigurationSection GetSection(string key) => throw new NotImplementedException();
        }
        private class TestVnPayConfigInvalidSignature : IConfiguration
        {
            public string this[string key]
            {
                get
                {
                    if (key == "VnPay:HashSecret") return "invalid";
                    return "test";
                }
                set => throw new NotImplementedException();
            }
            public IEnumerable<IConfigurationSection> GetChildren() => throw new NotImplementedException();
            public IChangeToken GetReloadToken() => null;
            public IConfigurationSection GetSection(string key) => throw new NotImplementedException();
        }
    }
} 