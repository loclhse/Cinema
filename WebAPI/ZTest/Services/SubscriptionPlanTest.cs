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
using System.Net;
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
        [Fact]
        public async Task CreateNewSubscriptionPlanAsync_ReturnsBadRequest_WhenExceptionOccurs()
        {
            // Arrange
            var request = new SubscriptionPlanRequest { Name = "Test" };
            var plan = new SubscriptionPlan { Id = Guid.NewGuid() };

            _mockMapper.Setup(m => m.Map<SubscriptionPlan>(request)).Returns(plan);
            _mockUnitOfWork.Setup(u => u.SubscriptionPlanRepo.AddAsync(plan)).ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _service.CreateNewSubscriptionPlanAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(null, result.ErrorMessage);
        }

        [Fact]
        public async Task ActiveSubscriptionPlanAsync_ReturnsBadRequest_WhenExceptionOccurs()
        {
            // Arrange
            var id = Guid.NewGuid();
            var plan = new SubscriptionPlan { Id = id, IsDeleted = false, Status = PlanStatus.Inactive };

            _mockUnitOfWork.Setup(u => u.SubscriptionPlanRepo.GetByIdAsync(id)).ReturnsAsync(plan);
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _service.ActiveSubscriptionPlanAsync(id);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(null, result.ErrorMessage);
        }

        [Fact]
        public async Task DeleteSubscriptionPlanAsync_ReturnsBadRequest_WhenExceptionOccurs()
        {
            // Arrange
            var id = Guid.NewGuid();
            var plan = new SubscriptionPlan { Id = id, IsDeleted = false };

            _mockUnitOfWork.Setup(u => u.SubscriptionPlanRepo.GetByIdAsync(id)).ReturnsAsync(plan);
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _service.DeleteSubscriptionPlanAsync(id);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(null, result.ErrorMessage);
        }

        [Fact]
        public async Task GetSubscriptionPlanByIdAsync_ReturnsBadRequest_WhenExceptionOccurs()
        {
            // Arrange
            var id = Guid.NewGuid();
            _mockUnitOfWork.Setup(u => u.SubscriptionPlanRepo.GetByIdAsync(id)).ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _service.GetSubscriptionPlanByIdAsync(id);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(null, result.ErrorMessage);
        }

        [Fact]
        public async Task UpdateInActiveSubscriptionPlanAsync_ReturnsBadRequest_WhenExceptionOccurs()
        {
            // Arrange
            var id = Guid.NewGuid();
            var request = new SubscriptionPlanRequest { Name = "Update" };
            var plan = new SubscriptionPlan { Id = id, Status = PlanStatus.Inactive };

            _mockUnitOfWork.Setup(u => u.SubscriptionPlanRepo.GetByIdAsync(id)).ReturnsAsync(plan);
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _service.UpdateInActiveSubscriptionPlanAsync(id, request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(null, result.ErrorMessage);
        }

        [Fact]
        public async Task UserGetAllSubscriptionPlansAsync_ReturnsBadRequest_WhenExceptionOccurs()
        {
            // Arrange
            _mockUnitOfWork.Setup(u => u.SubscriptionPlanRepo.GetAllAsync(It.IsAny<Expression<Func<SubscriptionPlan, bool>>>()))
                           .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _service.UserGetAllSubscriptionPlansAsync();

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(null, result.ErrorMessage);
        }

        [Fact]
        public async Task ManagerGetAllSubscriptionPlansHistoryAsync_ReturnsBadRequest_WhenExceptionOccurs()
        {
            // Arrange
            _mockUnitOfWork.Setup(u => u.SubscriptionPlanRepo.GetAllAsync(It.IsAny<Expression<Func<SubscriptionPlan, bool>>>()))
                           .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _service.ManagerGetAllSubscriptionPlansHistoryAsync();

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(null, result.ErrorMessage);
        }
        [Fact]
        public async Task ManagerGetAllSubscriptionPlansAsync_Should_Return_Ok_When_Plans_Exist()
        {
            // Arrange
            var subscriptionPlans = new List<SubscriptionPlan>
    {
        new SubscriptionPlan { Id = Guid.NewGuid(), Name = "Basic Plan" },
        new SubscriptionPlan { Id = Guid.NewGuid(), Name = "Premium Plan" }
    };

            _mockUnitOfWork.Setup(u => u.SubscriptionPlanRepo.GetAllAsync(It.IsAny<Expression<Func<SubscriptionPlan, bool>>>()))
                .ReturnsAsync(subscriptionPlans);

            var mappedResponse = new List<AdminSubPlanResponse>
    {
        new AdminSubPlanResponse { Id = subscriptionPlans[0].Id, Name = subscriptionPlans[0].Name },
        new AdminSubPlanResponse { Id = subscriptionPlans[1].Id, Name = subscriptionPlans[1].Name }
    };

            _mockMapper.Setup(m => m.Map<List<AdminSubPlanResponse>>(subscriptionPlans)).Returns(mappedResponse);

            // Act
            var result = await _service.ManagerGetAllSubscriptionPlansAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal(mappedResponse, result.Result);
        }

        [Fact]
        public async Task ManagerGetAllSubscriptionPlansAsync_Should_Return_NotFound_When_No_Plans_Found()
        {
            // Arrange
            _mockUnitOfWork.Setup(u => u.SubscriptionPlanRepo.GetAllAsync(It.IsAny<Expression<Func<SubscriptionPlan, bool>>>()))
                .ReturnsAsync(new List<SubscriptionPlan>()); // Simulate no plans found

            // Act
            var result = await _service.ManagerGetAllSubscriptionPlansAsync();

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
            Assert.Equal(null, result.ErrorMessage);
        }

        [Fact]
        public async Task ManagerGetAllSubscriptionPlansAsync_Should_Return_BadRequest_When_Exception_Occurs()
        {
            // Arrange
            _mockUnitOfWork.Setup(u => u.SubscriptionPlanRepo.GetAllAsync(It.IsAny<Expression<Func<SubscriptionPlan, bool>>>()))
                .ThrowsAsync(new Exception("Database error")); // Simulate a database error

            // Act
            var result = await _service.ManagerGetAllSubscriptionPlansAsync();

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            Assert.Equal(null, result.ErrorMessage);
        }
        [Fact]
        public async Task ActiveSubscriptionPlanAsync_Should_Return_NotFound_When_NonExistent()
        {
            // Arrange
            var id = Guid.NewGuid();
            _mockUnitOfWork.Setup(u => u.SubscriptionPlanRepo.GetByIdAsync(id)).ReturnsAsync((SubscriptionPlan)null);

            // Act
            var result = await _service.ActiveSubscriptionPlanAsync(id);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
            Assert.Equal(null, result.ErrorMessage);
        }

        [Fact]
        public async Task EditSubscriptionPlanAsync_Should_Return_NotFound_When_NonExistent()
        {
            // Arrange
            var id = Guid.NewGuid();
            var request = new SubscriptionPlanRequest();
            _mockUnitOfWork.Setup(u => u.SubscriptionPlanRepo.GetByIdAsync(id)).ReturnsAsync((SubscriptionPlan)null);

            // Act
            var result = await _service.UpdateInActiveSubscriptionPlanAsync(id, request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
            Assert.Equal(null, result.ErrorMessage);
        }

        [Fact]
        public async Task EditSubscriptionPlanAsync_Should_Return_BadRequest_When_Active()
        {
            // Arrange
            var id = Guid.NewGuid();
            var request = new SubscriptionPlanRequest();
            var activePlan = new SubscriptionPlan { Id = id, Status = PlanStatus.Active };

            _mockUnitOfWork.Setup(u => u.SubscriptionPlanRepo.GetByIdAsync(id)).ReturnsAsync(activePlan);

            // Act
            var result = await _service.UpdateInActiveSubscriptionPlanAsync(id, request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            Assert.Equal(null, result.ErrorMessage);
        }

        [Fact]
        public async Task CreateNewSubscriptionPlanAsync_Should_Return_BadRequest_When_Mapping_Fails()
        {
            // Arrange
            var request = new SubscriptionPlanRequest { Name = "Test" };
            _mockMapper.Setup(m => m.Map<SubscriptionPlan>(request)).Throws(new Exception("Mapping error"));

            // Act
            var result = await _service.CreateNewSubscriptionPlanAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            Assert.Equal(null, result.ErrorMessage);
        }

        [Fact]
        public async Task DeleteSubscriptionPlanAsync_Should_Return_NotFound_When_NonExistent()
        {
            // Arrange
            var id = Guid.NewGuid();
            _mockUnitOfWork.Setup(u => u.SubscriptionPlanRepo.GetByIdAsync(id)).ReturnsAsync((SubscriptionPlan)null);

            // Act
            var result = await _service.DeleteSubscriptionPlanAsync(id);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
            Assert.Equal(null, result.ErrorMessage);
        }

        [Fact]
        public async Task DeleteSubscriptionPlanAsync_Should_Return_BadRequest_When_Exception_Occurs()
        {
            // Arrange
            var id = Guid.NewGuid();
            var plan = new SubscriptionPlan { Id = id, IsDeleted = false };

            _mockUnitOfWork.Setup(u => u.SubscriptionPlanRepo.GetByIdAsync(id)).ReturnsAsync(plan);
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _service.DeleteSubscriptionPlanAsync(id);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            Assert.Equal(null, result.ErrorMessage);
        }
    }
}
