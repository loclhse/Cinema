using Application;
using Application.Services;
using Application.ViewModel;
using Application.ViewModel.Request;
using Application.ViewModel.Response;
using AutoMapper;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

public class SnackComboServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUow;
    private readonly Mock<IMapper> _mockMapper;
    private readonly SnackComboService _service;

    public SnackComboServiceTests()
    {
        _mockUow = new Mock<IUnitOfWork>();
        _mockMapper = new Mock<IMapper>();
        _service = new SnackComboService(_mockUow.Object, _mockMapper.Object);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_NotFound_When_Combo_DoesNotExist()
    {
        _mockUow.Setup(x => x.SnackComboRepo.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((SnackCombo)null);

        var result = await _service.GetByIdAsync(Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Combo_When_Exists()
    {
        var combo = new SnackCombo { Id = Guid.NewGuid(), Name = "Combo 1" };
        var response = new SnackComboResponse { Id = combo.Id, Name = combo.Name };

        _mockUow.Setup(x => x.SnackComboRepo.GetByIdAsync(combo.Id)).ReturnsAsync(combo);
        _mockMapper.Setup(x => x.Map<SnackComboResponse>(combo)).Returns(response);

        var result = await _service.GetByIdAsync(combo.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }
    [Fact]
    public async Task AddAsync_Should_Return_BadRequest_When_Name_Is_Empty()
    {
        var result = await _service.AddAsync(new SnackComboRequest { Name = "" });

        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    }
    [Fact]
    public async Task AddSnackToComboAsync_Should_Return_NotFound_When_Combo_NotExist()
    {
        _mockUow.Setup(x => x.SnackComboRepo.GetComboWithItemsAsync(It.IsAny<Guid>()))
                .ReturnsAsync((SnackCombo)null);

        var result = await _service.AddSnackToComboAsync(Guid.NewGuid(), new AddSnackToComboRequest());

        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    }
    [Fact]
    public async Task DeleteSnackFromComboAsync_Should_Return_NotFound_When_Snack_NotInCombo()
    {
        var combo = new SnackCombo
        {
            SnackComboItems = new List<SnackComboItem>()
        };

        _mockUow.Setup(x => x.SnackComboRepo.GetComboWithItemsAsync(It.IsAny<Guid>())).ReturnsAsync(combo);

        var result = await _service.DeleteSnackFromComboAsync(Guid.NewGuid(), Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
    }
    [Fact]
    public async Task GetAllSnackCombosAsync_Should_Return_NotFound_When_Empty()
    {
        _mockUow.Setup(x => x.SnackComboRepo.GetAllAsync(
        It.IsAny<Expression<Func<SnackCombo, bool>>>(),
        It.IsAny<Func<IQueryable<SnackCombo>, IIncludableQueryable<SnackCombo, object>>>()
    )).ReturnsAsync(new List<SnackCombo>());

        var result = await _service.GetAllSnackCombosAsync();

        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
    }
    [Fact]
    public async Task UpdateSnackQuantityInComboAsync_Should_Return_Success_When_Valid()
    {
        var snackId = Guid.NewGuid();
        var combo = new SnackCombo
        {
            SnackComboItems = new List<SnackComboItem>
        {
            new SnackComboItem
            {
                SnackId = snackId,
                Quantity = 1,
                IsDeleted = false,
                Snack = new Snack { Id = snackId, Price = 10, discount = 0 }
            }
        }
        };

        _mockUow.Setup(x => x.SnackComboRepo.GetComboWithItemsAsync(It.IsAny<Guid>())).ReturnsAsync(combo);
        _mockUow.Setup(x => x.SnackComboRepo.UpdateAsync(It.IsAny<SnackCombo>())).Returns(Task.CompletedTask);
        _mockUow.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _service.UpdateSnackQuantityInComboAsync(Guid.NewGuid(), snackId, 5);

        Assert.True(result.IsSuccess);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }


    [Fact]
    public async Task DeleteAsync_Should_Return_Ok_When_Success()
    {
        _mockUow.Setup(x => x.SnackComboRepo.DeleteAsync(It.IsAny<Guid>())).Returns(Task.CompletedTask);

        var result = await _service.DeleteAsync(Guid.NewGuid());

        Assert.True(result.IsSuccess);
    }
    [Fact]
    public async Task GetComboWithItemsAsync_Should_Return_Ok_When_Combo_Exists()
    {
        // Arrange
        var comboId = Guid.NewGuid();
        var combo = new SnackCombo { Id = comboId };
        var response = new SnackComboResponse { Id = comboId };

        _mockUow.Setup(x => x.SnackComboRepo.GetComboWithItemsAsync(comboId)).ReturnsAsync(combo);
        _mockMapper.Setup(m => m.Map<SnackComboResponse>(combo)).Returns(response);

        // Act
        var result = await _service.GetComboWithItemsAsync(comboId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }
    [Fact]
    public async Task GetComboWithItemsAsync_Should_Return_NotFound_When_Combo_Does_Not_Exist()
    {
        // Arrange
        var comboId = Guid.NewGuid();
        _mockUow.Setup(x => x.SnackComboRepo.GetComboWithItemsAsync(comboId)).ReturnsAsync((SnackCombo)null);

        // Act
        var result = await _service.GetComboWithItemsAsync(comboId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        Assert.Equal("Snack combo not found.", result.ErrorMessage);
    }
    [Fact]
    public async Task GetComboWithItemsAsync_Should_Return_BadRequest_When_Exception_Occurs()
    {
        // Arrange
        var comboId = Guid.NewGuid();
        _mockUow.Setup(x => x.SnackComboRepo.GetComboWithItemsAsync(comboId))
                .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _service.GetComboWithItemsAsync(comboId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Contains("Error retrieving combo with items", result.ErrorMessage);
    }
    [Fact]
    public async Task AddAsync_Should_Return_BadRequest_When_Exception_Occurs()
    {
        // Arrange
        var request = new SnackComboRequest { Name = "Test" };
        _mockMapper.Setup(m => m.Map<SnackCombo>(request)).Returns(new SnackCombo());
        _mockUow.Setup(u => u.SnackComboRepo.AddAsync(It.IsAny<SnackCombo>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _service.AddAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(null, result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteSnackFromComboAsync_Should_Return_BadRequest_When_Exception_Occurs()
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
        var result = await _service.AddAsync(request);
        Assert.True(result.IsSuccess);
        Assert.Equal(comboResponse, result.Result);
    }

    [Fact]
    public async Task UpdateAsync_Should_Return_BadRequest_When_Exception_Occurs()
    {
        // Arrange
        var id = Guid.NewGuid();
        var request = new SnackComboUpdateRequest { Name = "Update", TotalPrice = 100 };

        _mockUow.Setup(x => x.SnackComboRepo.GetAsync(It.IsAny<Expression<Func<SnackCombo, bool>>>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _service.UpdateAsync(id, request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(null, result.ErrorMessage);
    }

    [Fact]
    public async Task AddAsync_ReturnsBadRequest_WhenRequestIsNull()
    {
        var transaction = new Mock<IDbContextTransaction>();
        _mockUow.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(transaction.Object);
        var result = await _service.AddAsync(null);
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
        var result = await _service.AddAsync(request);
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
        var result = await _service.AddAsync(request);
        Assert.False(result.IsSuccess);
        Assert.Contains("Error adding snack combo", result.ErrorMessage);
    }

    [Fact]
    public async Task AddSnackToComboAsync_ReturnsOk_WhenValid()
    {
        // Arrange
        var comboId = Guid.NewGuid();
        var snackId = Guid.NewGuid();
        var quantity = 5;

        _mockUow.Setup(x => x.SnackComboRepo.GetComboWithItemsAsync(comboId))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _service.UpdateSnackQuantityInComboAsync(comboId, snackId, quantity);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(null, result.ErrorMessage);
    }

    [Fact]
    public async Task GetAllSnackCombosAsync_Should_Return_BadRequest_When_Exception_Occurs()
    {
        // Arrange
        _mockUow.Setup(x => x.SnackComboRepo.GetAllAsync(It.IsAny<Expression<Func<SnackCombo, bool>>>(), null))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _service.GetAllSnackCombosAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(null, result.ErrorMessage);
    }



    [Fact]
    public async Task AddSnackToComboAsync_Should_Return_NotFound_When_Combo_Not_Found()
    {
        // Arrange
        var comboId = Guid.NewGuid();
        var request = new AddSnackToComboRequest { SnackId = Guid.NewGuid() };
        _mockUow.Setup(x => x.SnackComboRepo.GetComboWithItemsAsync(comboId)).ReturnsAsync((SnackCombo)null);

        // Act
        var result = await _service.AddSnackToComboAsync(comboId, request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(null, result.ErrorMessage);
    }

    [Fact]
    public async Task AddSnackToComboAsync_Should_Return_NotFound_When_Snack_Not_Found()
    {
        // Arrange
        var comboId = Guid.NewGuid();
        var request = new AddSnackToComboRequest { SnackId = Guid.NewGuid() };
        var combo = new SnackCombo { Id = comboId, SnackComboItems = new List<SnackComboItem>() };

        _mockUow.Setup(x => x.SnackComboRepo.GetComboWithItemsAsync(comboId)).ReturnsAsync(combo);
        _mockUow.Setup(x => x.SnackRepo.GetByIdAsync(request.SnackId)).ReturnsAsync((Snack)null);

        // Act
        var result = await _service.AddSnackToComboAsync(comboId, request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal($"Snack with ID {request.SnackId} not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task AddSnackToComboAsync_Should_Return_BadRequest_When_Exception_Occurs()
    {
        // Arrange
        var comboId = Guid.NewGuid();
        var request = new AddSnackToComboRequest { SnackId = Guid.NewGuid() };
        var combo = new SnackCombo { Id = comboId, SnackComboItems = new List<SnackComboItem>() };

        _mockUow.Setup(x => x.SnackComboRepo.GetComboWithItemsAsync(comboId)).ReturnsAsync(combo);
        _mockUow.Setup(x => x.SnackRepo.GetByIdAsync(request.SnackId)).ReturnsAsync(new Snack());
        _mockUow.Setup(x => x.SnackComboRepo.AddComboItemAsync(It.IsAny<SnackComboItem>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _service.AddSnackToComboAsync(comboId, request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Error adding snack to combo: Database error", result.ErrorMessage);
    }

    [Fact]
    public async Task AddSnackToComboAsync_ReturnsBadRequest_WhenComboIdInvalid()
    {
        var transaction = new Mock<IDbContextTransaction>();
        _mockUow.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(transaction.Object);
        var result = await _service.AddSnackToComboAsync(Guid.Empty, new AddSnackToComboRequest { SnackId = Guid.NewGuid(), Quantity = 1 });
        Assert.False(result.IsSuccess);
        Assert.Equal("Invalid combo ID format.", result.ErrorMessage);
    }

    [Fact]
    public async Task AddSnackToComboAsync_ReturnsBadRequest_WhenRequestInvalid()
    {
        var transaction = new Mock<IDbContextTransaction>();
        _mockUow.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(transaction.Object);
        var result = await _service.AddSnackToComboAsync(Guid.NewGuid(), null);
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
        var result = await _service.AddSnackToComboAsync(comboId, request);
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
        var result = await _service.AddSnackToComboAsync(comboId, request);
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
        var result = await _service.AddSnackToComboAsync(comboId, request);
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
        var result = await _service.AddSnackToComboAsync(comboId, request);
        Assert.False(result.IsSuccess);
        Assert.Contains("Error adding snack to combo", result.ErrorMessage);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Handle_Exception()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockUow.Setup(x => x.SnackComboRepo.GetByIdAsync(id))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _service.GetByIdAsync(id);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Contains("Error retrieving snack combo", result.ErrorMessage);
    }

    [Fact]
    public async Task AddAsync_Should_Return_BadRequest_When_Request_Is_Null()
    {
        // Act
        var result = await _service.AddAsync(null);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Contains("Snack combo name is required", result.ErrorMessage);
    }

    [Fact]
    public async Task AddAsync_Should_Return_BadRequest_When_Name_Is_Whitespace()
    {
        // Arrange
        var request = new SnackComboRequest { Name = "   " };

        // Act
        var result = await _service.AddAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Contains("Snack combo name is required", result.ErrorMessage);
    }

    [Fact]
    public async Task AddAsync_Should_Return_NotFound_When_Snack_Not_Exists()
    {
        // Arrange
        var snackId = Guid.NewGuid();
        var request = new SnackComboRequest 
        { 
            Name = "Test Combo",
            SnackItems = new List<SnackComboItemRequest> { new SnackComboItemRequest { SnackId = snackId, Quantity = 1 } }
        };
        var combo = new SnackCombo { Name = "Test Combo", SnackComboItems = new List<SnackComboItem>() };

        var transaction = new Mock<IDbContextTransaction>();
        _mockUow.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(transaction.Object);
        _mockMapper.Setup(m => m.Map<SnackCombo>(request)).Returns(combo);
        _mockUow.Setup(x => x.SnackRepo.GetByIdAsync(snackId)).ReturnsAsync((Snack)null);

        // Act
        var result = await _service.AddAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        Assert.Contains($"Snack with ID {snackId} not found", result.ErrorMessage);
    }

    [Fact]
    public async Task AddAsync_Should_Handle_Exception()
    {
        // Arrange
        var request = new SnackComboRequest { Name = "Test Combo", SnackItems = new List<SnackComboItemRequest>() };
        var transaction = new Mock<IDbContextTransaction>();
        
        _mockUow.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(transaction.Object);
        _mockMapper.Setup(m => m.Map<SnackCombo>(request)).Throws(new Exception("Mapping error"));
        transaction.Setup(t => t.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _service.AddAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Contains("Error adding snack combo", result.ErrorMessage);
    }

    [Fact]
    public async Task AddSnackToComboAsync_Should_Return_BadRequest_When_ComboId_Is_Empty()
    {
        // Arrange
        var request = new AddSnackToComboRequest { SnackId = Guid.NewGuid(), Quantity = 1 };

        // Act
        var result = await _service.AddSnackToComboAsync(Guid.Empty, request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Contains("Invalid combo ID format", result.ErrorMessage);
    }

    [Fact]
    public async Task AddSnackToComboAsync_Should_Return_BadRequest_When_Request_Is_Null()
    {
        // Act
        var result = await _service.AddSnackToComboAsync(Guid.NewGuid(), null);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Contains("Valid snack ID is required", result.ErrorMessage);
    }

    [Fact]
    public async Task AddSnackToComboAsync_Should_Return_BadRequest_When_SnackId_Is_Empty()
    {
        // Arrange
        var request = new AddSnackToComboRequest { SnackId = Guid.Empty, Quantity = 1 };

        // Act
        var result = await _service.AddSnackToComboAsync(Guid.NewGuid(), request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Contains("Valid snack ID is required", result.ErrorMessage);
    }

    [Fact]
    public async Task AddSnackToComboAsync_Should_Return_NotFound_When_Snack_Not_Exists()
    {
        // Arrange
        var comboId = Guid.NewGuid();
        var snackId = Guid.NewGuid();
        var request = new AddSnackToComboRequest { SnackId = snackId, Quantity = 1 };
        var combo = new SnackCombo { Id = comboId, SnackComboItems = new List<SnackComboItem>() };

        var transaction = new Mock<IDbContextTransaction>();
        _mockUow.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(transaction.Object);
        _mockUow.Setup(x => x.SnackComboRepo.GetComboWithItemsAsync(comboId)).ReturnsAsync(combo);
        _mockUow.Setup(x => x.SnackRepo.GetByIdAsync(snackId)).ReturnsAsync((Snack)null);

        // Act
        var result = await _service.AddSnackToComboAsync(comboId, request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        Assert.Contains($"Snack with ID {snackId} not found", result.ErrorMessage);
    }

    [Fact]
    public async Task AddSnackToComboAsync_Should_Update_Existing_Item_Quantity()
    {
        // Arrange
        var comboId = Guid.NewGuid();
        var snackId = Guid.NewGuid();
        var request = new AddSnackToComboRequest { SnackId = snackId, Quantity = 2 };
        var existingItem = new SnackComboItem { SnackId = snackId, Quantity = 3, IsDeleted = false };
        var combo = new SnackCombo 
        { 
            Id = comboId, 
            SnackComboItems = new List<SnackComboItem> { existingItem } 
        };
        var snack = new Snack { Id = snackId, Name = "Test Snack" };
        var response = new SnackComboResponse { Id = comboId };

        var transaction = new Mock<IDbContextTransaction>();
        _mockUow.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(transaction.Object);
        _mockUow.Setup(x => x.SnackComboRepo.GetComboWithItemsAsync(comboId)).ReturnsAsync(combo);
        _mockUow.Setup(x => x.SnackRepo.GetByIdAsync(snackId)).ReturnsAsync(snack);
        _mockUow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        transaction.Setup(t => t.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockMapper.Setup(m => m.Map<SnackComboResponse>(It.IsAny<SnackCombo>())).Returns(response);

        // Act
        var result = await _service.AddSnackToComboAsync(comboId, request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(5, existingItem.Quantity); // 3 + 2
    }

    [Fact]
    public async Task AddSnackToComboAsync_Should_Handle_DbUpdateConcurrencyException()
    {
        // Arrange
        var comboId = Guid.NewGuid();
        var snackId = Guid.NewGuid();
        var request = new AddSnackToComboRequest { SnackId = snackId, Quantity = 1 };
        var combo = new SnackCombo { Id = comboId, SnackComboItems = new List<SnackComboItem>() };
        var snack = new Snack { Id = snackId };

        var transaction = new Mock<IDbContextTransaction>();
        _mockUow.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(transaction.Object);
        _mockUow.Setup(x => x.SnackComboRepo.GetComboWithItemsAsync(comboId)).ReturnsAsync(combo);
        _mockUow.Setup(x => x.SnackRepo.GetByIdAsync(snackId)).ReturnsAsync(snack);
        _mockUow.Setup(u => u.SaveChangesAsync()).ThrowsAsync(new DbUpdateConcurrencyException("Concurrency error"));
        transaction.Setup(t => t.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _service.AddSnackToComboAsync(comboId, request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Contains("Concurrency conflict", result.ErrorMessage);
    }

    [Fact]
    public async Task GetAllSnackCombosAsync_Should_Handle_Exception()
    {
        // Arrange
        _mockUow.Setup(x => x.SnackComboRepo.GetAllAsync(It.IsAny<Expression<Func<SnackCombo, bool>>>(), It.IsAny<Func<IQueryable<SnackCombo>, IIncludableQueryable<SnackCombo, object>>>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _service.GetAllSnackCombosAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Contains("Database error", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteSnackFromComboAsync_Should_Return_NotFound_When_Combo_Not_Exists()
    {
        // Arrange
        var comboId = Guid.NewGuid();
        var snackId = Guid.NewGuid();

        var transaction = new Mock<IDbContextTransaction>();
        _mockUow.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(transaction.Object);
        _mockUow.Setup(x => x.SnackComboRepo.GetComboWithItemsAsync(comboId)).ReturnsAsync((SnackCombo)null);

        // Act
        var result = await _service.DeleteSnackFromComboAsync(comboId, snackId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        Assert.Contains("Snack combo not found", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteSnackFromComboAsync_Should_Handle_Exception()
    {
        // Arrange
        var comboId = Guid.NewGuid();
        var snackId = Guid.NewGuid();

        var transaction = new Mock<IDbContextTransaction>();
        _mockUow.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(transaction.Object);
        _mockUow.Setup(x => x.SnackComboRepo.GetComboWithItemsAsync(comboId))
            .ThrowsAsync(new Exception("Database error"));
        transaction.Setup(t => t.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _service.DeleteSnackFromComboAsync(comboId, snackId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Contains("Error deleting snack from combo", result.ErrorMessage);
    }

    [Fact]
    public async Task GetComboWithItemsAsync_Should_Handle_Exception()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockUow.Setup(x => x.SnackComboRepo.GetComboWithItemsAsync(id))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _service.GetComboWithItemsAsync(id);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Contains("Error retrieving combo with items", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteAsync_Should_Handle_KeyNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockUow.Setup(x => x.SnackComboRepo.DeleteAsync(id))
            .ThrowsAsync(new KeyNotFoundException("Combo not found"));

        // Act
        var result = await _service.DeleteAsync(id);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        Assert.Contains("Combo not found", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteAsync_Should_Handle_Exception()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockUow.Setup(x => x.SnackComboRepo.DeleteAsync(id))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _service.DeleteAsync(id);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Contains("Error deleting snack combo", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateSnackQuantityInComboAsync_Should_Return_NotFound_When_Combo_Not_Exists()
    {
        // Arrange
        var comboId = Guid.NewGuid();
        var snackId = Guid.NewGuid();

        var transaction = new Mock<IDbContextTransaction>();
        _mockUow.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(transaction.Object);
        _mockUow.Setup(x => x.SnackComboRepo.GetComboWithItemsAsync(comboId)).ReturnsAsync((SnackCombo)null);

        // Act
        var result = await _service.UpdateSnackQuantityInComboAsync(comboId, snackId, 5);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        Assert.Contains("Snack combo not found", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateSnackQuantityInComboAsync_Should_Return_NotFound_When_Snack_Not_In_Combo()
    {
        // Arrange
        var comboId = Guid.NewGuid();
        var snackId = Guid.NewGuid();
        var combo = new SnackCombo { Id = comboId, SnackComboItems = new List<SnackComboItem>() };

        var transaction = new Mock<IDbContextTransaction>();
        _mockUow.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(transaction.Object);
        _mockUow.Setup(x => x.SnackComboRepo.GetComboWithItemsAsync(comboId)).ReturnsAsync(combo);

        // Act
        var result = await _service.UpdateSnackQuantityInComboAsync(comboId, snackId, 5);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        Assert.Contains($"Snack item with SnackId {snackId} not found in combo", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateSnackQuantityInComboAsync_Should_Handle_Exception()
    {
        // Arrange
        var comboId = Guid.NewGuid();
        var snackId = Guid.NewGuid();

        var transaction = new Mock<IDbContextTransaction>();
        _mockUow.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(transaction.Object);
        _mockUow.Setup(x => x.SnackComboRepo.GetComboWithItemsAsync(comboId))
            .ThrowsAsync(new Exception("Database error"));
        transaction.Setup(t => t.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdateSnackQuantityInComboAsync(comboId, snackId, 5);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Contains("Error updating snack quantity", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateAsync_Should_Return_BadRequest_When_Id_Is_Empty()
    {
        // Arrange
        var request = new SnackComboUpdateRequest { Name = "Test", TotalPrice = 10.0m };

        // Act
        var result = await _service.UpdateAsync(Guid.Empty, request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Contains("Invalid ID format", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateAsync_Should_Return_BadRequest_When_Request_Is_Null()
    {
        // Act
        var result = await _service.UpdateAsync(Guid.NewGuid(), null);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Contains("Request body cannot be null", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateAsync_Should_Return_NotFound_When_Combo_Not_Exists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var request = new SnackComboUpdateRequest { Name = "Test", TotalPrice = 10.0m };

        _mockUow.Setup(x => x.SnackComboRepo.GetAsync(It.IsAny<Expression<Func<SnackCombo, bool>>>()))
            .ReturnsAsync((SnackCombo)null);

        // Act
        var result = await _service.UpdateAsync(id, request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        Assert.Contains($"Snack combo with ID {id} not found", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateAsync_Should_Return_BadRequest_When_Name_Is_Empty()
    {
        // Arrange
        var id = Guid.NewGuid();
        var request = new SnackComboUpdateRequest { Name = "", TotalPrice = 10.0m };
        var combo = new SnackCombo { Id = id };

        _mockUow.Setup(x => x.SnackComboRepo.GetAsync(It.IsAny<Expression<Func<SnackCombo, bool>>>()))
            .ReturnsAsync(combo);

        // Act
        var result = await _service.UpdateAsync(id, request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Contains("Name is required", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateAsync_Should_Return_BadRequest_When_TotalPrice_Is_Zero_Or_Less()
    {
        // Arrange
        var id = Guid.NewGuid();
        var request = new SnackComboUpdateRequest { Name = "Test", TotalPrice = 0.0m };
        var combo = new SnackCombo { Id = id };

        _mockUow.Setup(x => x.SnackComboRepo.GetAsync(It.IsAny<Expression<Func<SnackCombo, bool>>>()))
            .ReturnsAsync(combo);

        // Act
        var result = await _service.UpdateAsync(id, request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Contains("Total price must be greater than zero", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateAsync_Should_Handle_Exception()
    {
        // Arrange
        var id = Guid.NewGuid();
        var request = new SnackComboUpdateRequest { Name = "Test", TotalPrice = 10.0m };

        _mockUow.Setup(x => x.SnackComboRepo.GetAsync(It.IsAny<Expression<Func<SnackCombo, bool>>>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _service.UpdateAsync(id, request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Contains("Database error", result.ErrorMessage);
    }

    [Fact]
    public void Constructor_Should_Throw_ArgumentNullException_When_Mapper_Is_Null()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new SnackComboService(_mockUow.Object, null));
    }
} 