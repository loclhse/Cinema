using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application;
using Application.Services;
using Application.ViewModel.Request;
using Application.ViewModel.Response;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;

public class SeatTypePriceServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUow;
    private readonly Mock<IMapper> _mockMapper;
    private readonly SeatTypePriceService _service;

    public SeatTypePriceServiceTests()
    {
        _mockUow = new Mock<IUnitOfWork>();
        _mockMapper = new Mock<IMapper>();
        _service = new SeatTypePriceService(_mockUow.Object, _mockMapper.Object);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsListOfSeatTypePriceResponse()
    {
        var entities = new List<SeatTypePrice> { new SeatTypePrice { Id = Guid.NewGuid(), SeatType = SeatTypes.Standard, DefaultPrice = 50 } };
        var responses = new List<SeatTypePriceResponse> { new SeatTypePriceResponse { Id = entities[0].Id, SeatType = SeatTypes.Standard, DefaultPrice = 50 } };
        _mockUow.Setup(u => u.SeatTypePriceRepo.GetAllAsync()).ReturnsAsync(entities);
        _mockMapper.Setup(m => m.Map<List<SeatTypePriceResponse>>(entities)).Returns(responses);
        var result = await _service.GetAllAsync();
        Assert.NotNull(result);
        Assert.Single(result);
    }

    [Fact]
    public async Task GetBySeatTypeAsync_ReturnsSeatTypePriceResponse_WhenExists()
    {
        var entity = new SeatTypePrice { Id = Guid.NewGuid(), SeatType = SeatTypes.VIP, DefaultPrice = 100 };
        var response = new SeatTypePriceResponse { Id = entity.Id, SeatType = SeatTypes.VIP, DefaultPrice = 100 };
        _mockUow.Setup(u => u.SeatTypePriceRepo.GetAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<SeatTypePrice, bool>>>())).ReturnsAsync(entity);
        _mockMapper.Setup(m => m.Map<SeatTypePriceResponse>(entity)).Returns(response);
        var result = await _service.GetBySeatTypeAsync(SeatTypes.VIP);
        Assert.NotNull(result);
        Assert.Equal(SeatTypes.VIP, result.SeatType);
    }

    [Fact]
    public async Task GetBySeatTypeAsync_ReturnsNull_WhenNotExists()
    {
        _mockUow.Setup(u => u.SeatTypePriceRepo.GetAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<SeatTypePrice, bool>>>())).ReturnsAsync((SeatTypePrice)null);
        var result = await _service.GetBySeatTypeAsync(SeatTypes.VIP);
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesPrice_WhenEntityExists()
    {
        var seatType = SeatTypes.Standard;
        var entity = new SeatTypePrice { Id = Guid.NewGuid(), SeatType = seatType, DefaultPrice = 50 };
        var request = new SeatTypePriceUpdateRequest { NewPrice = 80 };
        var response = new SeatTypePriceResponse { Id = entity.Id, SeatType = seatType, DefaultPrice = 80 };
        _mockUow.Setup(u => u.SeatTypePriceRepo.GetAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<SeatTypePrice, bool>>>())).ReturnsAsync(entity);
        _mockUow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _mockMapper.Setup(m => m.Map<SeatTypePriceResponse>(entity)).Returns(response);
        var result = await _service.UpdateAsync(seatType, request);
        Assert.NotNull(result);
        Assert.Equal(80, result.DefaultPrice);
    }

    [Fact]
    public async Task UpdateAsync_ThrowsKeyNotFound_WhenEntityNotExists()
    {
        var seatType = SeatTypes.Standard;
        var request = new SeatTypePriceUpdateRequest { NewPrice = 80 };
        _mockUow.Setup(u => u.SeatTypePriceRepo.GetAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<SeatTypePrice, bool>>>())).ReturnsAsync((SeatTypePrice)null);
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.UpdateAsync(seatType, request));
    }
} 