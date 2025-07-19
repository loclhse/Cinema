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
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

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
        var combo = new SnackCombo { Id = Guid.NewGuid(), Name = "Combo 2", TotalPrice = 100, SnackComboItems = new List<SnackComboItem>() };
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
    public async Task AddAsync_ReturnsBadRequest_WhenRequestIsNull()
    {
        var transaction = new Mock<IDbContextTransaction>();
        _mockUow.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(transaction.Object);
        var result = await _comboService.AddAsync(null);
        Assert.False(result.IsSuccess);
        Assert.Equal("Snack combo name is required.", result.ErrorMessage);
    }

    [Fact]
    public async Task AddAsync_ReturnsNotFound_WhenSnackNotFound()
    {
        var request = new SnackComboRequest { Name = "Combo", SnackItems = new List<SnackComboItemRequest> { new SnackComboItemRequest { SnackId = Guid.NewGuid(), Quantity = 1 } } };
        var combo = new SnackCombo { SnackComboItems = new List<SnackComboItem>() };
        var transaction = new Mock<IDbContextTransaction>();
        _mockUow.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(transaction.Object);
        _mockMapper.Setup(m => m.Map<SnackCombo>(request)).Returns(combo);
        _mockUow.Setup(u => u.SnackRepo.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Snack)null);
        var result = await _comboService.AddAsync(request);
        Assert.False(result.IsSuccess);
        Assert.Contains("Snack with ID", result.ErrorMessage);
    }

    [Fact]
    public async Task AddAsync_ReturnsBadRequest_OnException()
    {
        var request = new SnackComboRequest { Name = "Combo", SnackItems = new List<SnackComboItemRequest>() };
        var combo = new SnackCombo { SnackComboItems = new List<SnackComboItem>() };
        var transaction = new Mock<IDbContextTransaction>();
        _mockUow.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(transaction.Object);
        _mockMapper.Setup(m => m.Map<SnackCombo>(request)).Returns(combo);
        _mockUow.Setup(u => u.SnackRepo.GetByIdAsync(It.IsAny<Guid>())).ThrowsAsync(new Exception("db error"));
        transaction.Setup(t => t.RollbackAsync(default)).Returns(Task.CompletedTask);
        var result = await _comboService.AddAsync(request);
        Assert.False(result.IsSuccess);
        Assert.Contains("Error adding snack combo", result.ErrorMessage);
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

    [Fact]
    public async Task AddSnackToComboAsync_ReturnsBadRequest_WhenComboIdInvalid()
    {
        var transaction = new Mock<IDbContextTransaction>();
        _mockUow.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(transaction.Object);
        var result = await _comboService.AddSnackToComboAsync(Guid.Empty, new AddSnackToComboRequest { SnackId = Guid.NewGuid(), Quantity = 1 });
        Assert.False(result.IsSuccess);
        Assert.Equal("Invalid combo ID format.", result.ErrorMessage);
    }

    [Fact]
    public async Task AddSnackToComboAsync_ReturnsBadRequest_WhenRequestInvalid()
    {
        var transaction = new Mock<IDbContextTransaction>();
        _mockUow.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(transaction.Object);
        var result = await _comboService.AddSnackToComboAsync(Guid.NewGuid(), null);
        Assert.False(result.IsSuccess);
        Assert.Equal("Valid snack ID is required.", result.ErrorMessage);
    }

    [Fact]
    public async Task AddSnackToComboAsync_ReturnsNotFound_WhenSnackNotFound()
    {
        var comboId = Guid.NewGuid();
        var snackId = Guid.NewGuid();
        var combo = new SnackCombo { Id = comboId, SnackComboItems = new List<SnackComboItem>() };
        var request = new AddSnackToComboRequest { SnackId = snackId, Quantity = 1 };
        var transaction = new Mock<IDbContextTransaction>();
        _mockUow.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(transaction.Object);
        _mockUow.Setup(u => u.SnackComboRepo.GetComboWithItemsAsync(comboId)).ReturnsAsync(combo);
        _mockUow.Setup(u => u.SnackRepo.GetByIdAsync(snackId)).ReturnsAsync((Snack)null);
        var result = await _comboService.AddSnackToComboAsync(comboId, request);
        Assert.False(result.IsSuccess);
        Assert.Contains("Snack with ID", result.ErrorMessage);
    }

    [Fact]
    public async Task AddSnackToComboAsync_ReturnsOk_WhenItemExists()
    {
        var comboId = Guid.NewGuid();
        var snackId = Guid.NewGuid();
        var combo = new SnackCombo { Id = comboId, SnackComboItems = new List<SnackComboItem> { new SnackComboItem { SnackId = snackId, Quantity = 1, IsDeleted = false } } };
        var snack = new Snack { Id = snackId };
        var request = new AddSnackToComboRequest { SnackId = snackId, Quantity = 2 };
        var transaction = new Mock<IDbContextTransaction>();
        _mockUow.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(transaction.Object);
        _mockUow.Setup(u => u.SnackComboRepo.GetComboWithItemsAsync(comboId)).ReturnsAsync(combo);
        _mockUow.Setup(u => u.SnackRepo.GetByIdAsync(snackId)).ReturnsAsync(snack);
        _mockUow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        transaction.Setup(t => t.CommitAsync(default)).Returns(Task.CompletedTask);
        _mockUow.Setup(u => u.SnackComboRepo.GetComboWithItemsAsync(comboId)).ReturnsAsync(combo);
        _mockMapper.Setup(m => m.Map<SnackComboResponse>(combo)).Returns(new SnackComboResponse { Id = comboId });
        var result = await _comboService.AddSnackToComboAsync(comboId, request);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task AddSnackToComboAsync_ReturnsBadRequest_OnConcurrencyException()
    {
        var comboId = Guid.NewGuid();
        var snackId = Guid.NewGuid();
        var request = new AddSnackToComboRequest { SnackId = snackId, Quantity = 1 };
        var transaction = new Mock<IDbContextTransaction>();
        _mockUow.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(transaction.Object);
        _mockUow.Setup(u => u.SnackComboRepo.GetComboWithItemsAsync(comboId)).ThrowsAsync(new Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException("concurrency error"));
        transaction.Setup(t => t.RollbackAsync(default)).Returns(Task.CompletedTask);
        var result = await _comboService.AddSnackToComboAsync(comboId, request);
        Assert.False(result.IsSuccess);
        Assert.Contains("Concurrency conflict", result.ErrorMessage);
    }

    [Fact]
    public async Task AddSnackToComboAsync_ReturnsBadRequest_OnException()
    {
        var comboId = Guid.NewGuid();
        var snackId = Guid.NewGuid();
        var request = new AddSnackToComboRequest { SnackId = snackId, Quantity = 1 };
        var transaction = new Mock<IDbContextTransaction>();
        _mockUow.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(transaction.Object);
        _mockUow.Setup(u => u.SnackComboRepo.GetComboWithItemsAsync(comboId)).ThrowsAsync(new Exception("db error"));
        transaction.Setup(t => t.RollbackAsync(default)).Returns(Task.CompletedTask);
        var result = await _comboService.AddSnackToComboAsync(comboId, request);
        Assert.False(result.IsSuccess);
        Assert.Contains("Error adding snack to combo", result.ErrorMessage);
    }

    [Fact]
    public async Task GetAllSnackCombosAsync_ReturnsNotFound_WhenEmpty()
    {
        _mockUow.Setup(u => u.SnackComboRepo.GetAllAsync(It.IsAny<Expression<Func<SnackCombo, bool>>>(), It.IsAny<Func<IQueryable<SnackCombo>, IIncludableQueryable<SnackCombo, object>>>())).ReturnsAsync(new List<SnackCombo>());
        var result = await _comboService.GetAllSnackCombosAsync();
        Assert.False(result.IsSuccess);
        Assert.Equal("No snack combos found.", result.ErrorMessage);
    }

    [Fact]
    public async Task GetAllSnackCombosAsync_ReturnsBadRequest_OnException()
    {
        _mockUow.Setup(u => u.SnackComboRepo.GetAllAsync(It.IsAny<Expression<Func<SnackCombo, bool>>>(), It.IsAny<Func<IQueryable<SnackCombo>, IIncludableQueryable<SnackCombo, object>>>())).ThrowsAsync(new Exception("fail"));
        var result = await _comboService.GetAllSnackCombosAsync();
        Assert.False(result.IsSuccess);
        Assert.Equal("fail", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteSnackFromComboAsync_ReturnsNotFound_WhenComboNotFound()
    {
        _mockUow.Setup(u => u.SnackComboRepo.GetComboWithItemsAsync(It.IsAny<Guid>())).ReturnsAsync((SnackCombo)null);
        var result = await _comboService.DeleteSnackFromComboAsync(Guid.NewGuid(), Guid.NewGuid());
        Assert.False(result.IsSuccess);
        Assert.Equal("Snack combo not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteSnackFromComboAsync_ReturnsNotFound_WhenItemNotFound()
    {
        var combo = new SnackCombo { SnackComboItems = new List<SnackComboItem>() };
        _mockUow.Setup(u => u.SnackComboRepo.GetComboWithItemsAsync(It.IsAny<Guid>())).ReturnsAsync(combo);
        var result = await _comboService.DeleteSnackFromComboAsync(Guid.NewGuid(), Guid.NewGuid());
        Assert.False(result.IsSuccess);
        Assert.Contains("Snack item with SnackId", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteSnackFromComboAsync_ReturnsBadRequest_OnException()
    {
        var combo = new SnackCombo { SnackComboItems = new List<SnackComboItem>() };
        var transaction = new Mock<IDbContextTransaction>();
        _mockUow.Setup(u => u.SnackComboRepo.GetComboWithItemsAsync(It.IsAny<Guid>())).ThrowsAsync(new Exception("fail"));
        var result = await _comboService.DeleteSnackFromComboAsync(Guid.NewGuid(), Guid.NewGuid());
        Assert.False(result.IsSuccess);
        Assert.Contains("Error deleting snack from combo", result.ErrorMessage);
    }

    [Fact]
    public async Task GetComboWithItemsAsync_ReturnsNotFound_WhenNotFound()
    {
        _mockUow.Setup(u => u.SnackComboRepo.GetComboWithItemsAsync(It.IsAny<Guid>())).ReturnsAsync((SnackCombo)null);
        var result = await _comboService.GetComboWithItemsAsync(Guid.NewGuid());
        Assert.False(result.IsSuccess);
        Assert.Equal("Snack combo not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task GetComboWithItemsAsync_ReturnsBadRequest_OnException()
    {
        _mockUow.Setup(u => u.SnackComboRepo.GetComboWithItemsAsync(It.IsAny<Guid>())).ThrowsAsync(new Exception("fail"));
        var result = await _comboService.GetComboWithItemsAsync(Guid.NewGuid());
        Assert.False(result.IsSuccess);
        Assert.Contains("Error retrieving combo with items", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsNotFound_OnKeyNotFoundException()
    {
        _mockUow.Setup(u => u.SnackComboRepo.DeleteAsync(It.IsAny<Guid>())).Throws(new KeyNotFoundException("not found"));
        var result = await _comboService.DeleteAsync(Guid.NewGuid());
        Assert.False(result.IsSuccess);
        Assert.Equal("not found", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsBadRequest_OnException()
    {
        _mockUow.Setup(u => u.SnackComboRepo.DeleteAsync(It.IsAny<Guid>())).Throws(new Exception("fail"));
        var result = await _comboService.DeleteAsync(Guid.NewGuid());
        Assert.False(result.IsSuccess);
        Assert.Contains("Error deleting snack combo", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateSnackQuantityInComboAsync_ReturnsNotFound_WhenComboNotFound()
    {
        _mockUow.Setup(u => u.SnackComboRepo.GetComboWithItemsAsync(It.IsAny<Guid>())).ReturnsAsync((SnackCombo)null);
        var result = await _comboService.UpdateSnackQuantityInComboAsync(Guid.NewGuid(), Guid.NewGuid(), 2);
        Assert.False(result.IsSuccess);
        Assert.Equal("Snack combo not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateSnackQuantityInComboAsync_ReturnsNotFound_WhenItemNotFound()
    {
        var combo = new SnackCombo { SnackComboItems = new List<SnackComboItem>() };
        _mockUow.Setup(u => u.SnackComboRepo.GetComboWithItemsAsync(It.IsAny<Guid>())).ReturnsAsync(combo);
        var result = await _comboService.UpdateSnackQuantityInComboAsync(Guid.NewGuid(), Guid.NewGuid(), 2);
        Assert.False(result.IsSuccess);
        Assert.Contains("Snack item with SnackId", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateSnackQuantityInComboAsync_ReturnsBadRequest_OnException()
    {
        var combo = new SnackCombo { SnackComboItems = new List<SnackComboItem>() };
        var transaction = new Mock<IDbContextTransaction>();
        _mockUow.Setup(u => u.SnackComboRepo.GetComboWithItemsAsync(It.IsAny<Guid>())).ThrowsAsync(new Exception("fail"));
        var result = await _comboService.UpdateSnackQuantityInComboAsync(Guid.NewGuid(), Guid.NewGuid(), 2);
        Assert.False(result.IsSuccess);
        Assert.Contains("Error updating snack quantity", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsBadRequest_WhenIdInvalid()
    {
        var result = await _comboService.UpdateAsync(Guid.Empty, new SnackComboUpdateRequest());
        Assert.False(result.IsSuccess);
        Assert.Equal("Invalid ID format.", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsBadRequest_WhenRequestNull()
    {
        var result = await _comboService.UpdateAsync(Guid.NewGuid(), null);
        Assert.False(result.IsSuccess);
        Assert.Equal("Request body cannot be null.", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNotFound_WhenComboNotFound()
    {
        _mockUow.Setup(u => u.SnackComboRepo.GetAsync(It.IsAny<Expression<Func<SnackCombo, bool>>>())).ReturnsAsync((SnackCombo)null);
        var result = await _comboService.UpdateAsync(Guid.NewGuid(), new SnackComboUpdateRequest());
        Assert.False(result.IsSuccess);
        Assert.Contains("Snack combo with ID", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsBadRequest_WhenNameMissing()
    {
        var combo = new SnackCombo();
        _mockUow.Setup(u => u.SnackComboRepo.GetAsync(It.IsAny<Expression<Func<SnackCombo, bool>>>())).ReturnsAsync(combo);
        var request = new SnackComboUpdateRequest { Name = "", TotalPrice = 10 };
        var result = await _comboService.UpdateAsync(Guid.NewGuid(), request);
        Assert.False(result.IsSuccess);
        Assert.Equal("Name is required.", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsBadRequest_WhenPriceInvalid()
    {
        var combo = new SnackCombo();
        _mockUow.Setup(u => u.SnackComboRepo.GetAsync(It.IsAny<Expression<Func<SnackCombo, bool>>>())).ReturnsAsync(combo);
        var request = new SnackComboUpdateRequest { Name = "Combo", TotalPrice = 0 };
        var result = await _comboService.UpdateAsync(Guid.NewGuid(), request);
        Assert.False(result.IsSuccess);
        Assert.Equal("Total price must be greater than zero.", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsOk_WhenValid()
    {
        var combo = new SnackCombo();
        _mockUow.Setup(u => u.SnackComboRepo.GetAsync(It.IsAny<Expression<Func<SnackCombo, bool>>>())).ReturnsAsync(combo);
        var request = new SnackComboUpdateRequest { Name = "Combo", TotalPrice = 10 };
        _mockMapper.Setup(m => m.Map(request, combo)).Returns(combo);
        _mockUow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        var result = await _comboService.UpdateAsync(Guid.NewGuid(), request);
        Assert.True(result.IsSuccess);
        Assert.Equal("Snack combo updated successfully.", result.Result);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsBadRequest_OnException()
    {
        var combo = new SnackCombo();
        _mockUow.Setup(u => u.SnackComboRepo.GetAsync(It.IsAny<Expression<Func<SnackCombo, bool>>>())).Throws(new Exception("fail"));
        var request = new SnackComboUpdateRequest { Name = "Combo", TotalPrice = 10 };
        var result = await _comboService.UpdateAsync(Guid.NewGuid(), request);
        Assert.False(result.IsSuccess);
        Assert.Equal("fail", result.ErrorMessage);
    }
} 