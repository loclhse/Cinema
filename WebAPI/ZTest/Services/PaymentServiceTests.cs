using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Services;
using Application.ViewModel;
using Application.ViewModel.Response;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore.Query;
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

        _mockUow.Setup(u => u.PaymentRepo.UpdateAsync(payment))
            .Returns(Task.FromResult(0));

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
        Assert.Equal($"Payment with ID {payment.Id} is not in Pending status.", result.ErrorMessage);
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
        _mockUow.Setup(u => u.UserRepo.UpdateAsync(user)).Returns(Task.FromResult(0));
        _mockUow.Setup(u => u.PaymentRepo.UpdateAsync(payment)).Returns(Task.FromResult(0));
        _mockUow.Setup(u => u.OrderRepo.UpdateAsync(order)).Returns(Task.FromResult(0));
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
        _mockUow.Setup(u => u.PaymentRepo.UpdateAsync(payment)).Returns(Task.FromResult(0));
        _mockUow.Setup(u => u.OrderRepo.UpdateAsync(order)).Returns(Task.FromResult(0));
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
}