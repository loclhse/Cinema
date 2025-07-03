using Application;
using Application.IServices;
using Application.Services;
using Application.ViewModel;
using Application.ViewModel.Request;
using Application.ViewModel.Response;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;
namespace ZTest.Services
{
    public class SubscriptionServiceTests
    {
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly SubscriptionService _service;

        public SubscriptionServiceTests()
        {
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _mapperMock = new Mock<IMapper>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _service = new SubscriptionService(_httpContextAccessorMock.Object, _mapperMock.Object, _unitOfWorkMock.Object);
        }

        private void SetUserContext(Guid userId)
        {
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            var context = new DefaultHttpContext { User = claimsPrincipal };
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);
        }

        [Fact]
        public async Task CreateSubscription_ShouldReturnOk_WhenValid()
        {
            var plan = new SubscriptionPlan { Id = Guid.NewGuid(), Duration = 30, Name = "Basic", Price = 100 };
            var request = new SubscriptionRequest { SubscriptionPlanId = plan.Id };
            var userId = Guid.NewGuid();
            var subscription = new Subscription();

            SetUserContext(userId);

            _unitOfWorkMock.Setup(u => u.SubscriptionPlanRepo.GetByIdAsync(plan.Id)).ReturnsAsync(plan);
            _mapperMock.Setup(m => m.Map<Subscription>(request)).Returns(subscription);
            _unitOfWorkMock.Setup(u => u.SubscriptionRepo.AddAsync(It.IsAny<Subscription>())).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var result = await _service.CreateSubscription(request);

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task CreateSubscription_ShouldReturnUnauthorized_WhenNoUser()
        {
            var context = new DefaultHttpContext(); // empty user
            context.User = new ClaimsPrincipal(); // no claims at all

            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);

            var result = await _service.CreateSubscription(new SubscriptionRequest());

            Assert.False(result.IsSuccess);
            Assert.Equal(null, result.ErrorMessage);
        }

        [Fact]
        public async Task CancelSubscription_ShouldReturnOk_WhenFound()
        {
            var sub = new Subscription { Id = Guid.NewGuid(), IsDeleted = false };
            _unitOfWorkMock.Setup(u => u.SubscriptionRepo.GetAsync(It.IsAny<Expression<Func<Subscription, bool>>>())).ReturnsAsync(sub);

            var result = await _service.CancelSubscription(sub.Id);

            Assert.True(result.IsSuccess);
            Assert.Equal(SubscriptionStatus.cancelled, sub.Status);
        }

        [Fact]
        public async Task CancelSubscription_ShouldReturnNotFound_WhenMissing()
        {
            _unitOfWorkMock.Setup(u => u.SubscriptionRepo.GetAsync(It.IsAny<Expression<Func<Subscription, bool>>>())).ReturnsAsync((Subscription)null);

            var result = await _service.CancelSubscription(Guid.NewGuid());

            Assert.False(result.IsSuccess);
        }

        [Fact]
        public async Task DeleteSubscription_ShouldReturnOk_WhenFound()
        {
            var sub = new Subscription { Id = Guid.NewGuid() };
            _unitOfWorkMock.Setup(u => u.SubscriptionRepo.GetAsync(It.IsAny<Expression<Func<Subscription, bool>>>())).ReturnsAsync(sub);

            var result = await _service.DeleteSubscription(sub.Id);

            Assert.True(result.IsSuccess);
            Assert.True(sub.IsDeleted);
        }

        [Fact]
        public async Task GetAllSubscriptions_ShouldReturnOk_WhenDataExists()
        {
            var userId = Guid.NewGuid();
            SetUserContext(userId);

            var subs = new List<Subscription> { new Subscription { UserId = userId } };

            _unitOfWorkMock.Setup(u => u.SubscriptionRepo.GetAllAsync(It.IsAny<Expression<Func<Subscription, bool>>>()))
                           .ReturnsAsync(subs);
            _mapperMock.Setup(m => m.Map<List<SubscriptionResponse>>(subs))
                       .Returns(new List<SubscriptionResponse>());

            var result = await _service.GetAllSubscriptions();

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task GetAllSubscriptions_ShouldReturnUnauthorized_WhenNoUser()
        {
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(new DefaultHttpContext());

            var result = await _service.GetAllSubscriptions();

            Assert.False(result.IsSuccess);
            Assert.Equal("User not authenticated", result.ErrorMessage);
        }

        [Fact]
        public async Task GetSubscriptionById_ShouldReturnOk_WhenFound()
        {
            var sub = new Subscription();
            _unitOfWorkMock.Setup(u => u.SubscriptionRepo.GetAsync(It.IsAny<Expression<Func<Subscription, bool>>>())).ReturnsAsync(sub);
            _mapperMock.Setup(m => m.Map<SubscriptionResponse>(sub)).Returns(new SubscriptionResponse());

            var result = await _service.GetSubscriptionById(Guid.NewGuid());

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task UpdateSubscription_ShouldReturnOk_WhenValid()
        {
            var sub = new Subscription();
            var plan = new SubscriptionPlan { Duration = 30 };
            var req = new SubscriptionRequest { SubscriptionPlanId = Guid.NewGuid() };

            _unitOfWorkMock.Setup(u => u.SubscriptionRepo.GetAsync(It.IsAny<Expression<Func<Subscription, bool>>>())).ReturnsAsync(sub);
            _unitOfWorkMock.Setup(u => u.SubscriptionPlanRepo.GetByIdAsync(req.SubscriptionPlanId)).ReturnsAsync(plan);

            var result = await _service.UpdateSubscription(Guid.NewGuid(), req);

            Assert.True(result.IsSuccess);
            Assert.Equal(req.SubscriptionPlanId, sub.SubscriptionPlanId);
        }

        [Fact]
        public async Task UpdateSubscription_ShouldReturnNotFound_WhenSubscriptionMissing()
        {
            _unitOfWorkMock.Setup(u => u.SubscriptionRepo.GetAsync(It.IsAny<Expression<Func<Subscription, bool>>>())).ReturnsAsync((Subscription)null);

            var result = await _service.UpdateSubscription(Guid.NewGuid(), new SubscriptionRequest());

            Assert.False(result.IsSuccess);
            Assert.Equal(null, result.ErrorMessage);
        }

        [Fact]
        public async Task UpdateSubscription_ShouldReturnNotFound_WhenPlanMissing()
        {
            var subscriptionId = Guid.NewGuid();
            var missingPlanId = Guid.NewGuid();

            var sub = new Subscription();
            var request = new SubscriptionRequest { SubscriptionPlanId = missingPlanId };

            _unitOfWorkMock.Setup(u => u.SubscriptionRepo.GetAsync(It.IsAny<Expression<Func<Subscription, bool>>>())).ReturnsAsync(sub);
            _unitOfWorkMock.Setup(u => u.SubscriptionPlanRepo.GetByIdAsync(missingPlanId)).ReturnsAsync((SubscriptionPlan)null);

            var result = await _service.UpdateSubscription(subscriptionId, request);

            Assert.False(result.IsSuccess);
            Assert.Equal(null, result.ErrorMessage);
        }
    }
}
