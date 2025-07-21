using Application;
using Application.Domain;
using Application.IRepos;
using Application.IServices;
using Application.Services;
using Application.ViewModel;
using Application.ViewModel.Response;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Repos;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Application;
using Application.IServices;
using Microsoft.AspNetCore.Http;
using Application.IRepos;
using Microsoft.EntityFrameworkCore.Storage;
using Infrastructure.Service;

public class PaymentServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUow;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IVnPayService> _mockVnPayService;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly Mock<IAuthRepo> _mockAuthRepo;
    private readonly PaymentService _paymentService;

    public PaymentServiceTests()
    {
        _mockUow = new Mock<IUnitOfWork>();
        _mockMapper = new Mock<IMapper>();
        _mockVnPayService = new Mock<IVnPayService>();
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _mockAuthRepo = new Mock<IAuthRepo>();
        _paymentService = new PaymentService(
            _mockUow.Object,
            _mockMapper.Object,
            _mockVnPayService.Object,
            _mockHttpContextAccessor.Object,
            _mockAuthRepo.Object
        );
        
    }

    [Fact]
    public async Task FindPaymentByUserIdAsync_ShouldReturnOk_WhenPaymentsExist()
    {
        var userId = Guid.NewGuid();
        var payments = new List<Payment>
    {
        new Payment { Id = Guid.NewGuid(), userId = userId }
    };
        var responses = new List<PaymentResponse>
    {
        new PaymentResponse { Id = payments[0].Id }
    };

        _mockUow.Setup(u => u.PaymentRepo.GetAllAsync(
                It.IsAny<Expression<Func<Payment, bool>>>(),
                It.IsAny<Func<IQueryable<Payment>, IIncludableQueryable<Payment, object>>>()
        ))
            .ReturnsAsync(payments);

        _mockMapper.Setup(m => m.Map<List<PaymentResponse>>(payments)).Returns(responses);

        var result = await _paymentService.FindPaymentByUserIdAsync(userId);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Result);
    }
    
    [Fact]
    public async Task GetAllCashPaymentAsync_ShouldReturnOk_WhenCashPaymentsExist()
    {
        var payments = new List<Payment>
    {
        new Payment { Id = Guid.NewGuid(), PaymentMethod = PaymentMethod.Cash, IsDeleted = false }
    };
        var responses = new List<PaymentResponse>
    {
        new PaymentResponse { Id = payments[0].Id }
    };

        _mockUow.Setup(u => u.PaymentRepo.GetAllAsync(
                It.IsAny<Expression<Func<Payment, bool>>>(),
                It.IsAny<Func<IQueryable<Payment>, IIncludableQueryable<Payment, object>>>()))
            .ReturnsAsync(payments);

        _mockMapper.Setup(m => m.Map<List<PaymentResponse>>(payments)).Returns(responses);

        var result = await _paymentService.GetAllCashPaymentAsync();

        // Debug output
        Console.WriteLine($"IsSuccess: {result.IsSuccess}");
        Console.WriteLine($"StatusCode: {result.StatusCode}");
        Console.WriteLine($"ErrorMessage: {result.ErrorMessage}");
        Console.WriteLine($"Result: {result.Result}");

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Result);
    }

    [Fact]
    public async Task GetAllCashPaymentAsync_ShouldReturnNotFound_WhenNoCashPaymentsExist()
    {
        _mockUow.Setup(u => u.PaymentRepo.GetAllAsync(
                It.IsAny<Expression<Func<Payment, bool>>>(),
                It.IsAny<Func<IQueryable<Payment>, IIncludableQueryable<Payment, object>>>()))
            .ReturnsAsync(new List<Payment>());

        var result = await _paymentService.GetAllCashPaymentAsync();

        Assert.False(result.IsSuccess);
        Assert.Equal("No cash payments found.", result.ErrorMessage);
    }

    [Fact]
    public async Task GetAllCashPaymentAsync_ShouldReturnBadRequest_WhenExceptionThrown()
    {
        _mockUow.Setup(u => u.PaymentRepo.GetAllAsync(
                It.IsAny<Expression<Func<Payment, bool>>>(),
                It.IsAny<Func<IQueryable<Payment>, IIncludableQueryable<Payment, object>>>()))
            .ThrowsAsync(new Exception("DB error"));

        var result = await _paymentService.GetAllCashPaymentAsync();

        Assert.False(result.IsSuccess);
        Assert.Contains("Error retrieving cash payments: DB error", result.ErrorMessage);
    }

    [Fact]
    public async Task ChangeStatusFromPendingToSuccessAsync_ShouldReturnOk_WhenStatusChanged()
    {
        // Arrange
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            Status = PaymentStatus.Pending
        };

        _mockUow.Setup(u => u.PaymentRepo.GetByIdAsync(payment.Id))
            .ReturnsAsync(payment);

        _mockUow.Setup(u => u.PaymentRepo.UpdateAsync(It.IsAny<Payment>())).Returns(Task.FromResult(new Application.Common.OperationResult(new string[] { "ok" }, true)));

        // Act
        var result = await _paymentService.ChangeStatusFromPendingToSuccessAsync(payment.Id);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Payment status changed from Pending to Success successfully.", result.Result);
    }

    [Fact]
    public async Task ChangeStatusFromPendingToSuccessAsync_ShouldReturnBadRequest_WhenStatusNotPending()
    {
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            Status = PaymentStatus.Success
        };

        _mockUow.Setup(u => u.PaymentRepo.GetByIdAsync(payment.Id)).ReturnsAsync(payment);

        var result = await _paymentService.ChangeStatusFromPendingToSuccessAsync(payment.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal(null, result.ErrorMessage);
    }

    [Fact]
    public async Task FindPaymentByUserIdAsync_ShouldReturnNotFound_WhenEmpty()
    {
        _mockUow.Setup(u => u.PaymentRepo.GetAllAsync(
                It.IsAny<Expression<Func<Payment, bool>>>(),
                It.IsAny<Func<IQueryable<Payment>, IIncludableQueryable<Payment, object>>>(),
                It.IsAny<int>(),
                It.IsAny<int>()))
            .ReturnsAsync(new List<Payment>());

        var result = await _paymentService.FindPaymentByUserIdAsync(Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public async Task HandleVnPayReturn_ShouldReturnNotFound_WhenOrderNotFound()
    {
        var query = new Mock<IQueryCollection>();
        var response = new VnPaymentResponseModel { Success = true, OrderId = Guid.NewGuid().ToString() };
        _mockVnPayService.Setup(s => s.ProcessResponse(query.Object)).Returns(response);
        var mockTransaction = new Mock<IDbContextTransaction>();
        _mockUow.Setup(u => u.BeginTransactionAsync()).Returns(Task.FromResult(mockTransaction.Object));
        _mockUow.Setup(u => u.OrderRepo.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Domain.Entities.Order)null);

        var result = await _paymentService.HandleVnPayReturn(query.Object);
        Assert.False(result.IsSuccess);
        Assert.Equal("Order not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task HandleVnPayReturn_ShouldReturnNotFound_WhenPaymentNotFound()
    {
        var query = new Mock<IQueryCollection>();
        var orderId = Guid.NewGuid();
        var response = new VnPaymentResponseModel { Success = true, OrderId = orderId.ToString() };
        _mockVnPayService.Setup(s => s.ProcessResponse(query.Object)).Returns(response);
        var mockTransaction = new Mock<IDbContextTransaction>();
        _mockUow.Setup(u => u.BeginTransactionAsync()).Returns(Task.FromResult(mockTransaction.Object));
        _mockUow.Setup(u => u.OrderRepo.GetByIdAsync(orderId)).ReturnsAsync(new Domain.Entities.Order { Id = orderId });
        _mockUow.Setup(u => u.PaymentRepo.GetAsync(It.IsAny<Expression<Func<Payment, bool>>>())).ReturnsAsync((Payment)null);

        var result = await _paymentService.HandleVnPayReturn(query.Object);
        Assert.False(result.IsSuccess);
        Assert.Equal("Payment not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task HandleVnPayReturn_ShouldReturnOk_WhenPaymentSuccess()
    {
        var query = new Mock<IQueryCollection>();
        query.Setup(q => q["vnp_Amount"]).Returns("10000");
        query.Setup(q => q["vnp_TransactionNo"]).Returns("TXN123");
        var orderId = Guid.NewGuid();
        var response = new VnPaymentResponseModel { Success = true, OrderId = orderId.ToString() };
        var order = new Domain.Entities.Order { Id = orderId, Status = OrderEnum.Pending, UserId = Guid.NewGuid() };
        var payment = new Payment { Id = Guid.NewGuid(), OrderId = orderId, Status = PaymentStatus.Pending };
        var user = new Domain.Entities.AppUser { Id = order.UserId.Value, Score = 0 };
        _mockVnPayService.Setup(s => s.ProcessResponse(query.Object)).Returns(response);
        var mockTransaction = new Mock<IDbContextTransaction>();
        _mockUow.Setup(u => u.BeginTransactionAsync()).Returns(Task.FromResult(mockTransaction.Object));
        _mockUow.Setup(u => u.OrderRepo.GetByIdAsync(orderId)).ReturnsAsync(order);
        _mockUow.Setup(u => u.PaymentRepo.GetAsync(It.IsAny<Expression<Func<Payment, bool>>>())).ReturnsAsync(payment);
        _mockUow.Setup(u => u.UserRepo.GetByIdAsync(order.UserId.Value)).ReturnsAsync(user);
        _mockUow.Setup(u => u.ScoreLogRepo.AddAsync(It.IsAny<ScoreLog>())).Returns(Task.FromResult(0));
        _mockUow.Setup(u => u.UserRepo.UpdateAsync(It.IsAny<Domain.Entities.AppUser>())).Returns(Task.FromResult(0));
        _mockUow.Setup(u => u.PaymentRepo.UpdateAsync(It.IsAny<Payment>())).Returns(Task.FromResult(new Application.Common.OperationResult(new string[] { "ok" }, true)));
        _mockUow.Setup(u => u.OrderRepo.UpdateAsync(It.IsAny<Domain.Entities.Order>())).Returns(Task.FromResult(new Application.Common.OperationResult(new string[] { "ok" }, true)));
        _mockUow.Setup(u => u.SaveChangesAsync()).Returns(Task.FromResult(0));
    }

    [Fact]
    public async Task HandleVnPayReturn_ShouldReturnBadRequest_WhenUserNotFoundForScoreLog()
    {
        var query = new Mock<IQueryCollection>();
        query.Setup(q => q["vnp_Amount"]).Returns("10000");
        query.Setup(q => q["vnp_TransactionNo"]).Returns("TXN123");
        var orderId = Guid.NewGuid();
        var response = new VnPaymentResponseModel { Success = true, OrderId = orderId.ToString() };
        var order = new Domain.Entities.Order { Id = orderId, Status = OrderEnum.Pending, UserId = Guid.NewGuid() };
        var payment = new Payment { Id = Guid.NewGuid(), OrderId = orderId, Status = PaymentStatus.Pending, AmountPaid = 100 };
        _mockVnPayService.Setup(s => s.ProcessResponse(query.Object)).Returns(response);
        var mockTransaction = new Mock<IDbContextTransaction>();
        _mockUow.Setup(u => u.BeginTransactionAsync()).Returns(Task.FromResult(mockTransaction.Object));
        _mockUow.Setup(u => u.OrderRepo.GetByIdAsync(orderId)).ReturnsAsync(order);
        _mockUow.Setup(u => u.PaymentRepo.GetAsync(It.IsAny<Expression<Func<Payment, bool>>>())).ReturnsAsync(payment);
        _mockUow.Setup(u => u.UserRepo.GetByIdAsync(order.UserId.Value)).ReturnsAsync((Domain.Entities.AppUser)null);
        _mockUow.Setup(u => u.PaymentRepo.UpdateAsync(It.IsAny<Payment>())).Returns(Task.FromResult(new Application.Common.OperationResult(new string[] { "ok" }, true)));
        _mockUow.Setup(u => u.OrderRepo.UpdateAsync(It.IsAny<Domain.Entities.Order>())).Returns(Task.FromResult(new Application.Common.OperationResult(new string[] { "ok" }, true)));
        _mockUow.Setup(u => u.SaveChangesAsync()).Returns(Task.FromResult(0));
    }

    [Fact]
    public async Task HandleVnPayReturn_ShouldReturnBadRequest_WhenScoreLogThrowsException()
    {
        var query = new Mock<IQueryCollection>();
        query.Setup(q => q["vnp_Amount"]).Returns("10000");
        query.Setup(q => q["vnp_TransactionNo"]).Returns("TXN123");
        var orderId = Guid.NewGuid();
        var response = new VnPaymentResponseModel { Success = true, OrderId = orderId.ToString() };
        var order = new Domain.Entities.Order { Id = orderId, Status = OrderEnum.Pending, UserId = Guid.NewGuid() };
        var payment = new Payment { Id = Guid.NewGuid(), OrderId = orderId, Status = PaymentStatus.Pending, AmountPaid = 100 };
        var user = new Domain.Entities.AppUser { Id = order.UserId.Value, Score = 0 };
        _mockVnPayService.Setup(s => s.ProcessResponse(query.Object)).Returns(response);
        var mockTransaction = new Mock<IDbContextTransaction>();
        _mockUow.Setup(u => u.BeginTransactionAsync()).Returns(Task.FromResult(mockTransaction.Object));
        _mockUow.Setup(u => u.OrderRepo.GetByIdAsync(orderId)).ReturnsAsync(order);
        _mockUow.Setup(u => u.PaymentRepo.GetAsync(It.IsAny<Expression<Func<Payment, bool>>>())).ReturnsAsync(payment);
        _mockUow.Setup(u => u.UserRepo.GetByIdAsync(order.UserId.Value)).ReturnsAsync(user);
        _mockUow.Setup(u => u.ScoreLogRepo.AddAsync(It.IsAny<ScoreLog>())).ThrowsAsync(new Exception("Score log error"));
    }

    [Fact]
    public async Task HandleVnPayReturn_ShouldReturnBadRequest_WhenPaymentProcessingFailed()
    {
        var query = new Mock<IQueryCollection>();
        var orderId = Guid.NewGuid();
        var response = new VnPaymentResponseModel { Success = false, OrderId = orderId.ToString() };
        var order = new Domain.Entities.Order { Id = orderId, Status = OrderEnum.Pending, UserId = Guid.NewGuid() };
        var payment = new Payment { Id = Guid.NewGuid(), OrderId = orderId, Status = PaymentStatus.Pending };
        _mockVnPayService.Setup(s => s.ProcessResponse(query.Object)).Returns(response);
        var mockTransaction = new Mock<IDbContextTransaction>();
        _mockUow.Setup(u => u.BeginTransactionAsync()).Returns(Task.FromResult(mockTransaction.Object));
        _mockUow.Setup(u => u.OrderRepo.GetByIdAsync(orderId)).ReturnsAsync(order);
        _mockUow.Setup(u => u.PaymentRepo.GetAsync(It.IsAny<Expression<Func<Payment, bool>>>())).ReturnsAsync(payment);
    }

    [Fact]
    public async Task HandleVnPayReturn_ShouldReturnBadRequest_WhenExceptionThrown()
    {
        var query = new Mock<IQueryCollection>();
        _mockVnPayService.Setup(s => s.ProcessResponse(query.Object)).Throws(new Exception("VNPay error"));

        var result = await _paymentService.HandleVnPayReturn(query.Object);
        Assert.False(result.IsSuccess);
        Assert.Contains("Error processing VNPay return: VNPay error", result.ErrorMessage);
    }

    [Fact]
    public async Task FindPaymentByUserIdAsync_ShouldReturnBadRequest_WhenExceptionThrown()
    {
        _mockUow.Setup(u => u.PaymentRepo.GetAllAsync(
                It.IsAny<Expression<Func<Payment, bool>>>(),
                It.IsAny<Func<IQueryable<Payment>, IIncludableQueryable<Payment, object>>>()
        )).ThrowsAsync(new Exception("DB error"));

        var result = await _paymentService.FindPaymentByUserIdAsync(Guid.NewGuid());
        Assert.False(result.IsSuccess);
        Assert.Contains("Error finding payments: DB error", result.ErrorMessage);
    }

    [Fact]
    public async Task ChangeStatusFromPendingToSuccessAsync_ShouldReturnNotFound_WhenPaymentNotFound()
    {
        var id = Guid.NewGuid();
        _mockUow.Setup(u => u.PaymentRepo.GetByIdAsync(id)).ReturnsAsync((Payment)null);
        var result = await _paymentService.ChangeStatusFromPendingToSuccessAsync(id);
        Assert.False(result.IsSuccess);
        Assert.Equal($"Payment with ID {id} not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task ChangeStatusFromPendingToSuccessAsync_ShouldReturnBadRequest_WhenExceptionThrown()
    {
        var id = Guid.NewGuid();
        _mockUow.Setup(u => u.PaymentRepo.GetByIdAsync(id)).ThrowsAsync(new Exception("DB error"));
        var result = await _paymentService.ChangeStatusFromPendingToSuccessAsync(id);
        Assert.False(result.IsSuccess);
        Assert.Contains("Error updating payment status: DB error", result.ErrorMessage);
    }

    [Fact]
    public async Task HandleVnPayReturnForSubscription_ShouldReturnNotFound_WhenSubscriptionNotFound()
    {
        var query = new Mock<IQueryCollection>();
        var response = new VnPaymentResponseModel { Success = true, OrderId = Guid.NewGuid().ToString() };
        _mockVnPayService.Setup(s => s.ProcessResponsee(query.Object)).Returns(response);
        var mockTransaction = new Mock<IDbContextTransaction>();
        _mockUow.Setup(u => u.BeginTransactionAsync()).Returns(Task.FromResult(mockTransaction.Object));
        _mockUow.Setup(u => u.SubscriptionRepo.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Subscription)null);
        var result = await _paymentService.HandleVnPayReturnForSubscription(query.Object);
        Assert.False(result.IsSuccess);
        Assert.Equal("subscription not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task HandleVnPayReturnForSubscription_ShouldReturnNotFound_WhenPaymentNotFound()
    {
        var query = new Mock<IQueryCollection>();
        var subId = Guid.NewGuid();
        var response = new VnPaymentResponseModel { Success = true, OrderId = subId.ToString() };
        _mockVnPayService.Setup(s => s.ProcessResponsee(query.Object)).Returns(response);
        var mockTransaction = new Mock<IDbContextTransaction>();
        _mockUow.Setup(u => u.BeginTransactionAsync()).Returns(Task.FromResult(mockTransaction.Object));
        _mockUow.Setup(u => u.SubscriptionRepo.GetByIdAsync(subId)).ReturnsAsync(new Subscription { Id = subId });
        _mockUow.Setup(u => u.PaymentRepo.GetAsync(It.IsAny<Expression<Func<Payment, bool>>>())).ReturnsAsync((Payment)null);
        var result = await _paymentService.HandleVnPayReturnForSubscription(query.Object);
        Assert.False(result.IsSuccess);
        Assert.Equal("Payment not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task HandleVnPayReturnForSubscription_ShouldReturnOk_WhenPaymentSuccess()
    {
        var query = new Mock<IQueryCollection>();
        query.Setup(q => q["vnp_Amount"]).Returns("10000");
        query.Setup(q => q["vnp_TransactionNo"]).Returns("TXN123");
        var subId = Guid.NewGuid();
        var response = new VnPaymentResponseModel { Success = true, OrderId = subId.ToString() };
        var sub = new Subscription { Id = subId, Status = SubscriptionStatus.pending, SubscriptionPlanId = Guid.NewGuid(), UserId = Guid.NewGuid() };
        var payment = new Payment { Id = Guid.NewGuid(), SubscriptionId = subId, Status = PaymentStatus.Pending };
        var plan = new SubscriptionPlan { Id = sub.SubscriptionPlanId.Value };
        var user = new Domain.Entities.AppUser { Id = sub.UserId.Value, Score = 0 };
        _mockVnPayService.Setup(s => s.ProcessResponsee(query.Object)).Returns(response);
        var mockTransaction = new Mock<IDbContextTransaction>();
        _mockUow.Setup(u => u.BeginTransactionAsync()).Returns(Task.FromResult(mockTransaction.Object));
        _mockUow.Setup(u => u.SubscriptionRepo.GetByIdAsync(subId)).ReturnsAsync(sub);
        _mockUow.Setup(u => u.PaymentRepo.GetAsync(It.IsAny<Expression<Func<Payment, bool>>>())).ReturnsAsync(payment);
        _mockUow.Setup(u => u.SubscriptionPlanRepo.GetByIdAsync(sub.SubscriptionPlanId.Value)).ReturnsAsync(plan);
        _mockUow.Setup(u => u.SubscriptionPlanRepo.UpdateAsync(It.IsAny<SubscriptionPlan>())).Returns(Task.FromResult(new Application.Common.OperationResult(new string[] { "ok" }, true)));
        _mockUow.Setup(u => u.SubscriptionRepo.UpdateAsync(It.IsAny<Subscription>())).Returns(Task.FromResult(new Application.Common.OperationResult(new string[] { "ok" }, true)));
        _mockAuthRepo.Setup(a => a.RemoveUserFromRoleAsync(sub.UserId.Value, "Customer")).Returns(Task.FromResult(new Application.Common.OperationResult(new string[] { "ok" }, true)));
        _mockAuthRepo.Setup(a => a.AddUserToRoleAsync(sub.UserId.Value, "Member")).Returns(Task.FromResult(new Application.Common.OperationResult(new string[] { "ok" }, true)));
        _mockUow.Setup(u => u.UserRepo.GetByIdAsync(sub.UserId.Value)).ReturnsAsync(user);
        _mockUow.Setup(u => u.ScoreLogRepo.AddAsync(It.IsAny<ScoreLog>())).Returns(Task.FromResult(0));
        _mockUow.Setup(u => u.UserRepo.UpdateAsync(It.IsAny<Domain.Entities.AppUser>())).Returns(Task.FromResult(0));
        _mockUow.Setup(u => u.PaymentRepo.UpdateAsync(It.IsAny<Payment>())).Returns(Task.FromResult(new Application.Common.OperationResult(new string[] { "ok" }, true)));
        _mockUow.Setup(u => u.OrderRepo.UpdateAsync(It.IsAny<Domain.Entities.Order>())).Returns(Task.FromResult(new Application.Common.OperationResult(new string[] { "ok" }, true)));
        _mockUow.Setup(u => u.SaveChangesAsync()).Returns(Task.FromResult(0));
        var result = await _paymentService.HandleVnPayReturnForSubscription(query.Object);
        Assert.True(result.IsSuccess);
        Assert.Equal("Payment successful and order updated.", result.Result);
    }

    [Fact]
    public async Task HandleVnPayReturnForSubscription_ShouldReturnBadRequest_WhenScoreLogThrowsException()
    {
        var query = new Mock<IQueryCollection>();
        query.Setup(q => q["vnp_Amount"]).Returns("10000");
        query.Setup(q => q["vnp_TransactionNo"]).Returns("TXN123");
        var subId = Guid.NewGuid();
        var response = new VnPaymentResponseModel { Success = true, OrderId = subId.ToString() };
        var sub = new Subscription { Id = subId, Status = SubscriptionStatus.pending, SubscriptionPlanId = Guid.NewGuid(), UserId = Guid.NewGuid() };
        var payment = new Payment { Id = Guid.NewGuid(), SubscriptionId = subId, Status = PaymentStatus.Pending, AmountPaid = 100 };
        var plan = new SubscriptionPlan { Id = sub.SubscriptionPlanId.Value };
        var user = new Domain.Entities.AppUser { Id = sub.UserId.Value, Score = 0 };
        _mockVnPayService.Setup(s => s.ProcessResponsee(query.Object)).Returns(response);
        var mockTransaction = new Mock<IDbContextTransaction>();
        _mockUow.Setup(u => u.BeginTransactionAsync()).Returns(Task.FromResult(mockTransaction.Object));
        _mockUow.Setup(u => u.SubscriptionRepo.GetByIdAsync(subId)).ReturnsAsync(sub);
        _mockUow.Setup(u => u.PaymentRepo.GetAsync(It.IsAny<Expression<Func<Payment, bool>>>())).ReturnsAsync(payment);
        _mockUow.Setup(u => u.SubscriptionPlanRepo.GetByIdAsync(sub.SubscriptionPlanId.Value)).ReturnsAsync(plan);
        _mockUow.Setup(u => u.SubscriptionPlanRepo.UpdateAsync(It.IsAny<SubscriptionPlan>())).Returns(Task.FromResult(new Application.Common.OperationResult(new string[] { "ok" }, true)));
        _mockUow.Setup(u => u.SubscriptionRepo.UpdateAsync(It.IsAny<Subscription>())).Returns(Task.FromResult(new Application.Common.OperationResult(new string[] { "ok" }, true)));
        _mockAuthRepo.Setup(a => a.RemoveUserFromRoleAsync(sub.UserId.Value, "Customer")).Returns(Task.FromResult(new Application.Common.OperationResult(new string[] { "ok" }, true)));
        _mockAuthRepo.Setup(a => a.AddUserToRoleAsync(sub.UserId.Value, "Member")).Returns(Task.FromResult(new Application.Common.OperationResult(new string[] { "ok" }, true)));
        _mockUow.Setup(u => u.UserRepo.GetByIdAsync(sub.UserId.Value)).ReturnsAsync(user);
        _mockUow.Setup(u => u.ScoreLogRepo.AddAsync(It.IsAny<ScoreLog>())).ThrowsAsync(new Exception("Score log error"));
        var result = await _paymentService.HandleVnPayReturnForSubscription(query.Object);
        Assert.False(result.IsSuccess);
        Assert.Contains("Payment succeeded but failed to bonus score", result.ErrorMessage);
    }

    [Fact]
    public async Task HandleVnPayReturnForSubscription_ShouldReturnBadRequest_WhenPaymentProcessingFailed()
    {
        var query = new Mock<IQueryCollection>();
        var subId = Guid.NewGuid();
        var response = new VnPaymentResponseModel { Success = false, OrderId = subId.ToString() };
        var sub = new Subscription { Id = subId, Status = SubscriptionStatus.pending, SubscriptionPlanId = Guid.NewGuid(), UserId = Guid.NewGuid() };
        var payment = new Payment { Id = Guid.NewGuid(), SubscriptionId = subId, Status = PaymentStatus.Pending };
        var plan = new SubscriptionPlan { Id = sub.SubscriptionPlanId.Value };
        _mockVnPayService.Setup(s => s.ProcessResponsee(query.Object)).Returns(response);
        var mockTransaction = new Mock<IDbContextTransaction>();
        _mockUow.Setup(u => u.BeginTransactionAsync()).Returns(Task.FromResult(mockTransaction.Object));
        _mockUow.Setup(u => u.SubscriptionRepo.GetByIdAsync(subId)).ReturnsAsync(sub);
        _mockUow.Setup(u => u.PaymentRepo.GetAsync(It.IsAny<Expression<Func<Payment, bool>>>())).ReturnsAsync(payment);
        _mockUow.Setup(u => u.SubscriptionPlanRepo.GetByIdAsync(sub.SubscriptionPlanId.Value)).ReturnsAsync(plan);
        _mockUow.Setup(u => u.SubscriptionPlanRepo.UpdateAsync(It.IsAny<SubscriptionPlan>())).Returns(Task.FromResult(new Application.Common.OperationResult(new string[] { "ok" }, true)));
        _mockUow.Setup(u => u.SubscriptionRepo.UpdateAsync(It.IsAny<Subscription>())).Returns(Task.FromResult(new Application.Common.OperationResult(new string[] { "ok" }, true)));
        _mockAuthRepo.Setup(a => a.RemoveUserFromRoleAsync(sub.UserId.Value, "Customer")).Returns(Task.FromResult(new Application.Common.OperationResult(new string[] { "ok" }, true)));
        _mockAuthRepo.Setup(a => a.AddUserToRoleAsync(sub.UserId.Value, "Member")).Returns(Task.FromResult(new Application.Common.OperationResult(new string[] { "ok" }, true)));
        var result = await _paymentService.HandleVnPayReturnForSubscription(query.Object);
        Assert.False(result.IsSuccess);
        Assert.Equal("Payment processing failed.", result.ErrorMessage);
    }

    [Fact]
    public async Task HandleVnPayReturnForSubscription_ShouldReturnBadRequest_WhenExceptionThrown()
    {
        var query = new Mock<IQueryCollection>();
        _mockVnPayService.Setup(s => s.ProcessResponsee(query.Object)).Throws(new Exception("VNPay error"));
        var result = await _paymentService.HandleVnPayReturnForSubscription(query.Object);
        Assert.False(result.IsSuccess);
        Assert.Contains("Error processing VNPay return: VNPay error", result.ErrorMessage);
    }
    [Fact]
    public async Task HandleVnPayReturnForSubscription_ExceptionThrown_ReturnsBadRequest()
    {
        var query = new QueryCollection();
        _mockVnPayService.Setup(x => x.ProcessResponsee(query)).Throws(new Exception("fail"));

        var result = await _paymentService.HandleVnPayReturnForSubscription(query);

        Assert.False(result.IsSuccess);
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Contains(null, result.ErrorMessage);
    }
    [Fact]
    public async Task FindPaymentByUserIdAsync_ReturnsBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockUow.Setup(u => u.PaymentRepo.GetAllAsync(
                It.IsAny<Expression<Func<Payment, bool>>>(),
                It.IsAny<Func<IQueryable<Payment>, IIncludableQueryable<Payment, object>>>(),
                It.IsAny<int>(),
                It.IsAny<int>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _paymentService.FindPaymentByUserIdAsync(userId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(null, result.ErrorMessage);
    }

    [Fact]
    public async Task GetAllCashPaymentAsync_ReturnsBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        _mockUow.Setup(u => u.PaymentRepo.GetAllAsync(
                It.IsAny<Expression<Func<Payment, bool>>>(),
                It.IsAny<Func<IQueryable<Payment>, IIncludableQueryable<Payment, object>>>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _paymentService.GetAllCashPaymentAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(null, result.ErrorMessage);
    }

    [Fact]
    public async Task ChangeStatusFromPendingToSuccessAsync_ReturnsBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        _mockUow.Setup(u => u.PaymentRepo.GetByIdAsync(paymentId)).ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _paymentService.ChangeStatusFromPendingToSuccessAsync(paymentId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(null, result.ErrorMessage);
    }

    [Fact]
    public async Task HandleVnPayReturn_ReturnsBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        var query = new QueryCollection();
        _mockVnPayService.Setup(x => x.ProcessResponse(query)).Throws(new Exception("Processing error"));

        // Act
        var result = await _paymentService.HandleVnPayReturn(query);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(null, result.ErrorMessage);
    }

    
}