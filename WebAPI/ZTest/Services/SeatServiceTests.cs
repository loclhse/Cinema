using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application;
using Application.Services;
using Domain.Entities;
using Domain.Enums;

public class SeatServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUow;
    private readonly SeatService _seatService;

    public SeatServiceTests()
    {
        _mockUow = new Mock<IUnitOfWork>();
        _seatService = new SeatService(_mockUow.Object);
    }

    [Fact]
    public async Task GetSeatsByRoomAsync_ReturnsSeats_WhenSeatsExist()
    {
        var roomId = Guid.NewGuid();
        var seats = new List<Seat> { new Seat { Id = Guid.NewGuid(), CinemaRoomId = roomId, IsActive = true } };
        _mockUow.Setup(u => u.SeatRepo.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Seat, bool>>>()))
            .ReturnsAsync(seats);
        var result = await _seatService.GetSeatsByRoomAsync(roomId);
        Assert.NotNull(result);
        Assert.Single(result);
    }

    [Fact]
    public async Task GetSeatByIdAsync_ReturnsSeat_WhenSeatExists()
    {
        var seatId = Guid.NewGuid();
        var seat = new Seat { Id = seatId, IsActive = true };
        _mockUow.Setup(u => u.SeatRepo.GetAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Seat, bool>>>()))
            .ReturnsAsync(seat);
        var result = await _seatService.GetSeatByIdAsync(seatId);
        Assert.NotNull(result);
        Assert.Equal(seatId, result.Id);
    }

    [Fact]
    public async Task UpdateSeatTypeAsync_UpdatesSeatType_ForGivenSeats()
    {
        var seatIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var seats = seatIds.Select(id => new Seat { Id = id, SeatType = SeatTypes.Standard }).ToList();
        _mockUow.Setup(u => u.SeatRepo.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Seat, bool>>>()))
            .ReturnsAsync(seats);
        _mockUow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        await _seatService.UpdateSeatTypeAsync(seatIds, SeatTypes.VIP);
        Assert.All(seats, s => Assert.Equal(SeatTypes.VIP, s.SeatType));
    }

    [Fact]
    public async Task UpdateSeatAvailabilityAsync_UpdatesAvailability_ForGivenSeats()
    {
        var seatIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var seats = seatIds.Select(id => new Seat { Id = id, IsAvailable = false }).ToList();
        _mockUow.Setup(u => u.SeatRepo.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Seat, bool>>>()))
            .ReturnsAsync(seats);
        _mockUow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        await _seatService.UpdateSeatAvailabilityAsync(seatIds, true);
        Assert.All(seats, s => Assert.True(s.IsAvailable));
    }

    [Fact]
    public async Task GetSeatsByRoomAsync_ReturnsEmpty_WhenNoSeatsExist()
    {
        var roomId = Guid.NewGuid();
        _mockUow.Setup(u => u.SeatRepo.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Seat, bool>>>())).ReturnsAsync(new List<Seat>());
        var result = await _seatService.GetSeatsByRoomAsync(roomId);
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetSeatsByRoomAsync_ThrowsException_OnRepoError()
    {
        var roomId = Guid.NewGuid();
        _mockUow.Setup(u => u.SeatRepo.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Seat, bool>>>())).ThrowsAsync(new Exception("fail"));
        await Assert.ThrowsAsync<Exception>(() => _seatService.GetSeatsByRoomAsync(roomId));
    }

    [Fact]
    public async Task GetSeatByIdAsync_ReturnsNull_WhenSeatNotFound()
    {
        var seatId = Guid.NewGuid();
        _mockUow.Setup(u => u.SeatRepo.GetAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Seat, bool>>>())).ReturnsAsync((Seat)null);
        var result = await _seatService.GetSeatByIdAsync(seatId);
        Assert.Null(result);
    }

    [Fact]
    public async Task GetSeatByIdAsync_ThrowsException_OnRepoError()
    {
        var seatId = Guid.NewGuid();
        _mockUow.Setup(u => u.SeatRepo.GetAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Seat, bool>>>())).ThrowsAsync(new Exception("fail"));
        await Assert.ThrowsAsync<Exception>(() => _seatService.GetSeatByIdAsync(seatId));
    }

    [Fact]
    public async Task UpdateSeatTypeAsync_DoesNothing_WhenNoSeatsFound()
    {
        var seatIds = new List<Guid> { Guid.NewGuid() };
        _mockUow.Setup(u => u.SeatRepo.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Seat, bool>>>())).ReturnsAsync(new List<Seat>());
        _mockUow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        await _seatService.UpdateSeatTypeAsync(seatIds, SeatTypes.VIP);
        // No exception, no update
    }

    [Fact]
    public async Task UpdateSeatTypeAsync_ThrowsException_OnRepoError()
    {
        var seatIds = new List<Guid> { Guid.NewGuid() };
        _mockUow.Setup(u => u.SeatRepo.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Seat, bool>>>())).ThrowsAsync(new Exception("fail"));
        await Assert.ThrowsAsync<Exception>(() => _seatService.UpdateSeatTypeAsync(seatIds, SeatTypes.VIP));
    }

    [Fact]
    public async Task UpdateSeatAvailabilityAsync_DoesNothing_WhenNoSeatsFound()
    {
        var seatIds = new List<Guid> { Guid.NewGuid() };
        _mockUow.Setup(u => u.SeatRepo.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Seat, bool>>>())).ReturnsAsync(new List<Seat>());
        _mockUow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        await _seatService.UpdateSeatAvailabilityAsync(seatIds, true);
        // No exception, no update
    }

    [Fact]
    public async Task UpdateSeatAvailabilityAsync_ThrowsException_OnRepoError()
    {
        var seatIds = new List<Guid> { Guid.NewGuid() };
        _mockUow.Setup(u => u.SeatRepo.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Seat, bool>>>())).ThrowsAsync(new Exception("fail"));
        await Assert.ThrowsAsync<Exception>(() => _seatService.UpdateSeatAvailabilityAsync(seatIds, true));
    }
}