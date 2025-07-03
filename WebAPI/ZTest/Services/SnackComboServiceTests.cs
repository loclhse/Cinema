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
using Microsoft.EntityFrameworkCore.Storage;

public class SnackComboServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUow;
    private readonly Mock<IMapper> _mockMapper;
    private readonly SnackComboService _comboService;

    public SnackComboServiceTests()
    {
        _mockUow = new Mock<IUnitOfWork>();
        _mockMapper = new Mock<IMapper>();
        _comboService = new SnackComboService(_mockUow.Object, _mockMapper.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsOk_WhenComboExists()
    {
        var comboId = Guid.NewGuid();
        var combo = new SnackCombo { Id = comboId, Name = "Combo 1" };
        var comboResponse = new SnackComboResponse { Id = comboId, Name = "Combo 1" };
        _mockUow.Setup(u => u.SnackComboRepo.GetByIdAsync(comboId)).ReturnsAsync(combo);
        _mockMapper.Setup(m => m.Map<SnackComboResponse>(combo)).Returns(comboResponse);
        var result = await _comboService.GetByIdAsync(comboId);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Result);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNotFound_WhenComboDoesNotExist()
    {
        var comboId = Guid.NewGuid();
        _mockUow.Setup(u => u.SnackComboRepo.GetByIdAsync(comboId)).ReturnsAsync((SnackCombo)null);
        var result = await _comboService.GetByIdAsync(comboId);
        Assert.False(result.IsSuccess);
        Assert.Equal("Snack combo not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task AddAsync_ReturnsCreated_WhenValidRequest()
    {
        var request = new SnackComboRequest { Name = "Combo 2", TotalPrice = 100, SnackItems = new List<SnackComboItemRequest>() };
        var combo = new SnackCombo { Id = Guid.NewGuid(), Name = "Combo 2", TotalPrice = 100 };
        var comboResponse = new SnackComboResponse { Id = combo.Id, Name = "Combo 2" };
        var transaction = new Mock<IDbContextTransaction>();
        _mockMapper.Setup(m => m.Map<SnackCombo>(request)).Returns(combo);
        _mockUow.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(transaction.Object);
        _mockUow.Setup(u => u.SnackComboRepo.AddAsync(combo)).Returns(Task.CompletedTask);
        _mockUow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _mockMapper.Setup(m => m.Map<SnackComboResponse>(combo)).Returns(comboResponse);
        transaction.Setup(t => t.CommitAsync(default)).Returns(Task.CompletedTask);
        var result = await _comboService.AddAsync(request);
        Assert.True(result.IsSuccess);
        Assert.Equal(comboResponse, result.Result);
    }

    [Fact]
    public async Task AddAsync_ReturnsBadRequest_WhenNameMissing()
    {
        var request = new SnackComboRequest { Name = "", TotalPrice = 100, SnackItems = new List<SnackComboItemRequest>() };
        var transaction = new Mock<IDbContextTransaction>();
        _mockUow.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(transaction.Object);
        var result = await _comboService.AddAsync(request);
        Assert.False(result.IsSuccess);
        Assert.Equal("Snack combo name is required.", result.ErrorMessage);
    }

    [Fact]
    public async Task AddSnackToComboAsync_ReturnsOk_WhenValid()
    {
        var comboId = Guid.NewGuid();
        var snackId = Guid.NewGuid();
        var combo = new SnackCombo { Id = comboId, Name = "Combo 3", SnackComboItems = new List<SnackComboItem>() };
        var snack = new Snack { Id = snackId, Name = "Chips" };
        var request = new AddSnackToComboRequest { SnackId = snackId, Quantity = 2 };
        var transaction = new Mock<IDbContextTransaction>();
        _mockUow.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(transaction.Object);
        _mockUow.Setup(u => u.SnackComboRepo.GetComboWithItemsAsync(comboId)).ReturnsAsync(combo);
        _mockUow.Setup(u => u.SnackRepo.GetByIdAsync(snackId)).ReturnsAsync(snack);
        _mockUow.Setup(u => u.SnackComboRepo.AddComboItemAsync(It.IsAny<SnackComboItem>())).Returns(Task.CompletedTask);
        _mockUow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        transaction.Setup(t => t.CommitAsync(default)).Returns(Task.CompletedTask);
        _mockUow.Setup(u => u.SnackComboRepo.GetComboWithItemsAsync(comboId)).ReturnsAsync(combo);
        _mockMapper.Setup(m => m.Map<SnackComboResponse>(combo)).Returns(new SnackComboResponse { Id = comboId, Name = "Combo 3" });
        var result = await _comboService.AddSnackToComboAsync(comboId, request);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Result);
    }

    [Fact]
    public async Task AddSnackToComboAsync_ReturnsNotFound_WhenComboDoesNotExist()
    {
        var comboId = Guid.NewGuid();
        var snackId = Guid.NewGuid();
        var request = new AddSnackToComboRequest { SnackId = snackId, Quantity = 2 };
        var transaction = new Mock<IDbContextTransaction>();
        _mockUow.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(transaction.Object);
        _mockUow.Setup(u => u.SnackComboRepo.GetComboWithItemsAsync(comboId)).ReturnsAsync((SnackCombo)null);
        var result = await _comboService.AddSnackToComboAsync(comboId, request);
        Assert.False(result.IsSuccess);
        Assert.Equal("Snack combo not found.", result.ErrorMessage);
    }
} 