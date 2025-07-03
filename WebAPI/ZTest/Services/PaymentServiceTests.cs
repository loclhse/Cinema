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

public class PaymentServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUow;
    private readonly Mock<IMapper> _mockMapper;
    private readonly PaymentService _paymentService;

    public PaymentServiceTests()
    {
        _mockUow = new Mock<IUnitOfWork>();
        _mockMapper = new Mock<IMapper>();
        _paymentService = new PaymentService(_mockUow.Object, _mockMapper.Object);
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
                It.IsAny<Func<IQueryable<Payment>, IIncludableQueryable<Payment, object>>>(),
                It.IsAny<int>(),
                It.IsAny<int>()))
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
        new Payment { Id = Guid.NewGuid(), PaymentMethod = PaymentMethod.Cash }
    };
        var responses = new List<PaymentResponse>
    {
        new PaymentResponse { Id = payments[0].Id }
    };

        _mockUow.Setup(u => u.PaymentRepo.GetAllAsync(
                It.IsAny<Expression<Func<Payment, bool>>>(),
                It.IsAny<Func<IQueryable<Payment>, IIncludableQueryable<Payment, object>>>(),
                It.IsAny<int>(),
                It.IsAny<int>()))
            .ReturnsAsync(payments);

        _mockMapper.Setup(m => m.Map<List<PaymentResponse>>(payments)).Returns(responses);

        var result = await _paymentService.GetAllCashPaymentAsync();

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Result);
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
            .Returns(Task.CompletedTask);

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
        Assert.Equal(null, result.ErrorMessage);
    }
}
