using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Services;
using Application.ViewModel;
using Application.ViewModel.Response;
using Application.ViewModel.Request;
using AutoMapper;
using Domain.Entities;
using Application;

public class SnackServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUow;
    private readonly Mock<IMapper> _mockMapper;
    private readonly SnackService _snackService;

    public SnackServiceTests()
    {
        _mockUow = new Mock<IUnitOfWork>();
        _mockMapper = new Mock<IMapper>();
        _snackService = new SnackService(_mockUow.Object, _mockMapper.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsOk_WhenSnackExists()
    {
        var snackId = Guid.NewGuid();
        var snack = new Snack { Id = snackId, Name = "Popcorn" };
        var snackResponse = new SnackResponse { Id = snackId, Name = "Popcorn" };
        _mockUow.Setup(u => u.SnackRepo.GetByIdAsync(snackId)).ReturnsAsync(snack);
        _mockMapper.Setup(m => m.Map<SnackResponse>(snack)).Returns(snackResponse);
        var result = await _snackService.GetByIdAsync(snackId);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Result);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNotFound_WhenSnackDoesNotExist()
    {
        var snackId = Guid.NewGuid();
        _mockUow.Setup(u => u.SnackRepo.GetByIdAsync(snackId)).ReturnsAsync((Snack)null);
        var result = await _snackService.GetByIdAsync(snackId);
        Assert.False(result.IsSuccess);
        Assert.Equal("Snack not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task AddAsync_ReturnsCreated_WhenValidRequest()
    {
        var request = new SnackRequest { Name = "Chips", Quantity = 10 };
        var snack = new Snack { Id = Guid.NewGuid(), Name = "Chips" };
        var snackResponse = new SnackResponse { Id = snack.Id, Name = "Chips" };
        _mockMapper.Setup(m => m.Map<Snack>(request)).Returns(snack);
        _mockUow.Setup(u => u.SnackRepo.AddAsync(snack)).Returns(Task.CompletedTask);
        _mockUow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _mockMapper.Setup(m => m.Map<SnackResponse>(snack)).Returns(snackResponse);
        var result = await _snackService.AddAsync(request);
        Assert.True(result.IsSuccess);
        Assert.Equal(snackResponse, result.Result);
    }

    [Fact]
    public async Task AddAsync_ReturnsBadRequest_WhenNameMissing()
    {
        var request = new SnackRequest { Name = "", Quantity = 10 };
        var result = await _snackService.AddAsync(request);
        Assert.False(result.IsSuccess);
        Assert.Equal("Snack name is required.", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsOk_WhenSnackExists()
    {
        var snackId = Guid.NewGuid();
        var request = new SnackRequest { Name = "Updated", Quantity = 5 };
        var snack = new Snack { Id = snackId, Name = "Old" };
        var snackResponse = new SnackResponse { Id = snackId, Name = "Updated" };
        _mockUow.Setup(u => u.SnackRepo.GetByIdAsync(snackId)).ReturnsAsync(snack);
        _mockMapper.Setup(m => m.Map(request, snack)).Returns(snack);
        _mockUow.Setup(u => u.SnackRepo.UpdateAsync(snack)).Returns(Task.CompletedTask);
        _mockMapper.Setup(m => m.Map<SnackResponse>(snack)).Returns(snackResponse);
        var result = await _snackService.UpdateAsync(snackId, request);
        Assert.True(result.IsSuccess);
        Assert.Equal(snackResponse, result.Result);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNotFound_WhenSnackDoesNotExist()
    {
        var snackId = Guid.NewGuid();
        var request = new SnackRequest { Name = "Updated", Quantity = 5 };
        _mockUow.Setup(u => u.SnackRepo.GetByIdAsync(snackId)).ReturnsAsync((Snack)null);
        var result = await _snackService.UpdateAsync(snackId, request);
        Assert.False(result.IsSuccess);
        Assert.Equal("Snack not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsOk_WhenSnackDeleted()
    {
        var snackId = Guid.NewGuid();
        _mockUow.Setup(u => u.SnackRepo.DeleteAsync(snackId)).Returns(Task.CompletedTask);
        var result = await _snackService.DeleteAsync(snackId);
        Assert.True(result.IsSuccess);
        Assert.Equal("Snack deleted successfully.", result.Result);
    }
} 