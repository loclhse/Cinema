using Application;
using Application.Services;
using Application.ViewModel;
using Application.ViewModel.Request;
using Application.ViewModel.Response;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ZTest.Services
{
    public class SubscriptionPlanTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMapper> _mockMapper;
        private readonly SubscriptionPlanService _service;
        public SubscriptionPlanTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _service = new SubscriptionPlanService(_mockUnitOfWork.Object, _mockMapper.Object);
        }
        [Fact]
        public async Task CreateNewSubscriptionPlanAsync_ShouldReturnOk_WhenValid()
        {
            var request = new SubscriptionPlanRequest { Name = "Test" };
            var plan = new SubscriptionPlan { Id = Guid.NewGuid() };
            _mockMapper.Setup(m => m.Map<SubscriptionPlan>(request)).Returns(plan);
            _mockUnitOfWork.Setup(u => u.SubscriptionPlanRepo.AddAsync(plan)).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var result = await _service.CreateNewSubscriptionPlanAsync(request);

            Assert.True(result.IsSuccess);
            Assert.Equal(System.Net.HttpStatusCode.OK, result.StatusCode);
        }

        [Fact]
        public async Task GetSubscriptionPlanByIdAsync_ShouldReturnOk_WhenFound()
        {
            var id = Guid.NewGuid();
            var plan = new SubscriptionPlan { Id = id };
            _mockUnitOfWork.Setup(u => u.SubscriptionPlanRepo.GetByIdAsync(id)).ReturnsAsync(plan);
            _mockMapper.Setup(m => m.Map<SubscriptionPlanResponse>(plan)).Returns(new SubscriptionPlanResponse { Id = id });

            var result = await _service.GetSubscriptionPlanByIdAsync(id);

            Assert.True(result.IsSuccess);
            Assert.Equal(System.Net.HttpStatusCode.OK, result.StatusCode);
        }

        [Fact]
        public async Task GetSubscriptionPlanByIdAsync_ShouldReturnNotFound_WhenNull()
        {
            var id = Guid.NewGuid();
            _mockUnitOfWork.Setup(u => u.SubscriptionPlanRepo.GetByIdAsync(id)).ReturnsAsync((SubscriptionPlan)null);

            var result = await _service.GetSubscriptionPlanByIdAsync(id);

            Assert.False(result.IsSuccess);
            Assert.Equal(System.Net.HttpStatusCode.NotFound, result.StatusCode);
        }

        [Fact]
        public async Task DeleteSubscriptionPlanAsync_ShouldReturnOk_WhenValid()
        {
            var id = Guid.NewGuid();
            var plan = new SubscriptionPlan { Id = id, IsDeleted = false };
            _mockUnitOfWork.Setup(u => u.SubscriptionPlanRepo.GetByIdAsync(id)).ReturnsAsync(plan);
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var result = await _service.DeleteSubscriptionPlanAsync(id);

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task ActiveSubscriptionPlanAsync_ShouldReturnOk_WhenValid()
        {
            var id = Guid.NewGuid();
            var plan = new SubscriptionPlan { Id = id, Status = PlanStatus.Inactive, IsDeleted = false };
            _mockUnitOfWork.Setup(u => u.SubscriptionPlanRepo.GetByIdAsync(id)).ReturnsAsync(plan);
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var result = await _service.ActiveSubscriptionPlanAsync(id);

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task ActiveSubscriptionPlanAsync_ShouldReturnNotFound_WhenAlreadyActiveOrDeleted()
        {
            var id = Guid.NewGuid();
            var plan = new SubscriptionPlan { Id = id, Status = PlanStatus.Active, IsDeleted = false };
            _mockUnitOfWork.Setup(u => u.SubscriptionPlanRepo.GetByIdAsync(id)).ReturnsAsync(plan);
            var result = await _service.ActiveSubscriptionPlanAsync(id);
            Assert.False(result.IsSuccess);
            Assert.Equal(System.Net.HttpStatusCode.NotFound, result.StatusCode);
        }

        [Fact]
        public async Task ActiveSubscriptionPlanAsync_ShouldReturnBadRequest_OnException()
        {
            var id = Guid.NewGuid();
            _mockUnitOfWork.Setup(u => u.SubscriptionPlanRepo.GetByIdAsync(id)).Throws(new Exception("fail"));
            var result = await _service.ActiveSubscriptionPlanAsync(id);
            Assert.False(result.IsSuccess);
            Assert.Equal("fail", result.ErrorMessage);
        }

        [Fact]
        public async Task CreateNewSubscriptionPlanAsync_ShouldReturnBadRequest_WhenInvalid()
        {
            var request = new SubscriptionPlanRequest();
            _mockMapper.Setup(m => m.Map<SubscriptionPlan>(request)).Returns((SubscriptionPlan)null);
            var result = await _service.CreateNewSubscriptionPlanAsync(request);
            Assert.False(result.IsSuccess);
            Assert.Contains("Invalid subscription plan data", result.ErrorMessage);
        }

        [Fact]
        public async Task CreateNewSubscriptionPlanAsync_ShouldReturnBadRequest_OnException()
        {
            var request = new SubscriptionPlanRequest();
            _mockMapper.Setup(m => m.Map<SubscriptionPlan>(request)).Throws(new Exception("fail"));
            var result = await _service.CreateNewSubscriptionPlanAsync(request);
            Assert.False(result.IsSuccess);
            Assert.Equal("fail", result.ErrorMessage);
        }

        [Fact]
        public async Task DeleteSubscriptionPlanAsync_ShouldReturnNotFound_WhenNotFoundOrDeleted()
        {
            var id = Guid.NewGuid();
            _mockUnitOfWork.Setup(u => u.SubscriptionPlanRepo.GetByIdAsync(id)).ReturnsAsync((SubscriptionPlan)null);
            var result = await _service.DeleteSubscriptionPlanAsync(id);
            Assert.False(result.IsSuccess);
            Assert.Equal(System.Net.HttpStatusCode.NotFound, result.StatusCode);
        }

        [Fact]
        public async Task DeleteSubscriptionPlanAsync_ShouldReturnBadRequest_OnException()
        {
            var id = Guid.NewGuid();
            _mockUnitOfWork.Setup(u => u.SubscriptionPlanRepo.GetByIdAsync(id)).Throws(new Exception("fail"));
            var result = await _service.DeleteSubscriptionPlanAsync(id);
            Assert.False(result.IsSuccess);
            Assert.Equal("fail", result.ErrorMessage);
        }

        [Fact]
        public async Task UpdateInActiveSubscriptionPlanAsync_ShouldReturnOk_WhenValid()
        {
            var id = Guid.NewGuid();
            var plan = new SubscriptionPlan { Id = id, Status = PlanStatus.Inactive };
            var request = new SubscriptionPlanRequest { Name = "Update" };

            _mockUnitOfWork.Setup(u => u.SubscriptionPlanRepo.GetByIdAsync(id)).ReturnsAsync(plan);
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var result = await _service.UpdateInActiveSubscriptionPlanAsync(id, request);

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task UpdateInActiveSubscriptionPlanAsync_ShouldReturnNotFound_WhenNotFoundOrDeleted()
        {
            var id = Guid.NewGuid();
            _mockUnitOfWork.Setup(u => u.SubscriptionPlanRepo.GetByIdAsync(id)).ReturnsAsync((SubscriptionPlan)null);
            var result = await _service.UpdateInActiveSubscriptionPlanAsync(id, new SubscriptionPlanRequest());
            Assert.False(result.IsSuccess);
            Assert.Equal(System.Net.HttpStatusCode.NotFound, result.StatusCode);
        }

        [Fact]
        public async Task UpdateInActiveSubscriptionPlanAsync_ShouldReturnBadRequest_WhenActive()
        {
            var id = Guid.NewGuid();
            var plan = new SubscriptionPlan { Id = id, Status = PlanStatus.Active };
            _mockUnitOfWork.Setup(u => u.SubscriptionPlanRepo.GetByIdAsync(id)).ReturnsAsync(plan);
            var result = await _service.UpdateInActiveSubscriptionPlanAsync(id, new SubscriptionPlanRequest());
            Assert.False(result.IsSuccess);
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task UpdateInActiveSubscriptionPlanAsync_ShouldReturnBadRequest_OnException()
        {
            var id = Guid.NewGuid();
            _mockUnitOfWork.Setup(u => u.SubscriptionPlanRepo.GetByIdAsync(id)).Throws(new Exception("fail"));
            var result = await _service.UpdateInActiveSubscriptionPlanAsync(id, new SubscriptionPlanRequest());
            Assert.False(result.IsSuccess);
            Assert.Equal("fail", result.ErrorMessage);
        }

        [Fact]
        public async Task UserGetAllSubscriptionPlansAsync_ShouldReturnOk_WhenFound()
        {
            // Arrange
            var plans = new List<SubscriptionPlan> { new SubscriptionPlan() };

            _mockUnitOfWork
                .Setup(u => u.SubscriptionPlanRepo.GetAllAsync(It.IsAny<Expression<Func<SubscriptionPlan, bool>>>()))
                .ReturnsAsync(plans);

            _mockMapper
                .Setup(m => m.Map<List<SubscriptionPlanResponse>>(plans))
                .Returns(new List<SubscriptionPlanResponse> { new SubscriptionPlanResponse() });

            // Act
            var result = await _service.UserGetAllSubscriptionPlansAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.IsType<ApiResp>(result);
        }

        [Fact]
        public async Task UserGetAllSubscriptionPlansAsync_ShouldReturnNotFound_WhenEmpty()
        {
            _mockUnitOfWork.Setup(u => u.SubscriptionPlanRepo.GetAllAsync(It.IsAny<Expression<Func<SubscriptionPlan, bool>>>())).ReturnsAsync(new List<SubscriptionPlan>());
            var result = await _service.UserGetAllSubscriptionPlansAsync();
            Assert.False(result.IsSuccess);
            Assert.Equal(System.Net.HttpStatusCode.NotFound, result.StatusCode);
        }

        [Fact]
        public async Task UserGetAllSubscriptionPlansAsync_ShouldReturnBadRequest_OnException()
        {
            _mockUnitOfWork.Setup(u => u.SubscriptionPlanRepo.GetAllAsync(It.IsAny<Expression<Func<SubscriptionPlan, bool>>>())).Throws(new Exception("fail"));
            var result = await _service.UserGetAllSubscriptionPlansAsync();
            Assert.False(result.IsSuccess);
            Assert.Equal("fail", result.ErrorMessage);
        }

        [Fact]
        public async Task ManagerGetAllSubscriptionPlansHistoryAsync_ShouldReturnNotFound_WhenEmpty()
        {
            _mockUnitOfWork.Setup(u => u.SubscriptionPlanRepo.GetAllAsync(It.IsAny<Expression<Func<SubscriptionPlan, bool>>>())).ReturnsAsync(new List<SubscriptionPlan>());
            var result = await _service.ManagerGetAllSubscriptionPlansHistoryAsync();
            Assert.False(result.IsSuccess);
            Assert.Equal(System.Net.HttpStatusCode.NotFound, result.StatusCode);
        }

        [Fact]
        public async Task ManagerGetAllSubscriptionPlansHistoryAsync_ShouldReturnBadRequest_OnException()
        {
            _mockUnitOfWork.Setup(u => u.SubscriptionPlanRepo.GetAllAsync(It.IsAny<Expression<Func<SubscriptionPlan, bool>>>())).Throws(new Exception("fail"));
            var result = await _service.ManagerGetAllSubscriptionPlansHistoryAsync();
            Assert.False(result.IsSuccess);
            Assert.Equal("fail", result.ErrorMessage);
        }
    }
}
