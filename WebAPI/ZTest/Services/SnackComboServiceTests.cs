using Application;
using Application.Services;
using Application.ViewModel;
using Application.ViewModel.Request;
using Application.ViewModel.Response;
using AutoMapper;
using Domain.Entities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net;
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
    public async Task UpdateAsync_Should_Return_BadRequest_When_Name_Is_Empty()
    {
        var id = Guid.NewGuid();
        var request = new SnackComboUpdateRequest { Name = "", TotalPrice = 100 };

        _mockUow.Setup(x => x.SnackComboRepo.GetAsync(It.IsAny<Expression<Func<SnackCombo, bool>>>()))
        .ReturnsAsync(new SnackCombo { Id = id });

        var result = await _service.UpdateAsync(id, request);

        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
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
        // Arrange
        var comboId = Guid.NewGuid();
        var snackId = Guid.NewGuid();
        _mockUow.Setup(x => x.SnackComboRepo.GetComboWithItemsAsync(comboId))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _service.DeleteSnackFromComboAsync(comboId, snackId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(null, result.ErrorMessage);
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
    public async Task UpdateSnackQuantityInComboAsync_Should_Return_BadRequest_When_Exception_Occurs()
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
    public async Task AddSnackToComboAsync_Should_Return_BadRequest_When_ComboId_Is_Empty()
    {
        // Arrange
        var request = new AddSnackToComboRequest { SnackId = Guid.NewGuid() };

        // Act
        var result = await _service.AddSnackToComboAsync(Guid.Empty, request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(null, result.ErrorMessage);
    }

    [Fact]
    public async Task AddSnackToComboAsync_Should_Return_BadRequest_When_Request_Is_Null()
    {
        // Act
        var result = await _service.AddSnackToComboAsync(Guid.NewGuid(), null);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(null, result.ErrorMessage);
    }

    [Fact]
    public async Task AddSnackToComboAsync_Should_Return_BadRequest_When_SnackId_Is_Empty()
    {
        // Arrange
        var request = new AddSnackToComboRequest { SnackId = Guid.Empty };

        // Act
        var result = await _service.AddSnackToComboAsync(Guid.NewGuid(), request);

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
}