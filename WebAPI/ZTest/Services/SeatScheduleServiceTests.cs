using Application;
using Application.Common;
using Application.IRepos;
using Application.IServices;
using Application.Services;
using Application.ViewModel.Response;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests.Services
{
    public class SeatScheduleServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUow;
        private readonly Mock<ISeatScheduleRepo> _mockSeatScheduleRepo;
        private readonly Mock<IMapper> _mockMapper;
        private readonly SeatScheduleService _service;

        public SeatScheduleServiceTests()
        {
            _mockUow = new Mock<IUnitOfWork>();
            _mockSeatScheduleRepo = new Mock<ISeatScheduleRepo>();
            _mockMapper = new Mock<IMapper>();

            _mockUow.Setup(u => u.SeatScheduleRepo).Returns(_mockSeatScheduleRepo.Object);

            _service = new SeatScheduleService(_mockUow.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task GetHoldSeatByUserIdAsync_ReturnsMappedSeats_WhenSeatsFound()
        {
            // Arrange
            var showtimeId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var seats = new List<SeatSchedule> { new SeatSchedule { Id = Guid.NewGuid(), Status = SeatBookingStatus.Hold, HoldByUserId = userId } };
            var mappedSeats = new List<SeatScheduleResponse> { new SeatScheduleResponse { Id = seats[0].Id } };

            _mockSeatScheduleRepo.Setup(r => r.GetAllAsync(It.IsAny<Expression<Func<SeatSchedule, bool>>>()))
                .ReturnsAsync(seats);
            _mockMapper.Setup(m => m.Map<List<SeatScheduleResponse>>(seats)).Returns(mappedSeats);

            // Act
            var result = await _service.GetHoldSeatByUserIdAsync(showtimeId, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public async Task GetSeatSchedulesByShowtimeAsync_ReturnsMappedSeats_WhenSeatsExist()
        {
            var showtimeId = Guid.NewGuid();
            var seatSchedule = new SeatSchedule { Id = Guid.NewGuid(), ShowtimeId = showtimeId };
            var seatScheduleResponse = new SeatScheduleResponse { Id = seatSchedule.Id };
            _mockUow.Setup(u => u.SeatScheduleRepo.GetAllAsync(
                It.IsAny<Expression<Func<SeatSchedule, bool>>>(),
                It.IsAny<Func<IQueryable<SeatSchedule>, IIncludableQueryable<SeatSchedule, object>>>()
            )).ReturnsAsync(new List<SeatSchedule> { seatSchedule });
            _mockMapper.Setup(m => m.Map<IEnumerable<SeatScheduleResponse>>(It.IsAny<List<SeatSchedule>>()))
                .Returns(new List<SeatScheduleResponse> { seatScheduleResponse });
            var result = await _service.GetSeatSchedulesByShowtimeAsync(showtimeId);
            Assert.Single(result);
        }

        [Fact]
        public async Task UpdateSeatStatusAsync_UpdatesStatusAndSaves_WhenValid()
        {
            // Arrange
            var seatIds = new List<Guid> { Guid.NewGuid() };
            var seats = new List<SeatSchedule> { new SeatSchedule { Id = seatIds[0] } };

            _mockSeatScheduleRepo.Setup(r => r.GetAllAsync(It.IsAny<Expression<Func<SeatSchedule, bool>>>()))
                .ReturnsAsync(seats);

            _mockUow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _service.UpdateSeatStatusAsync(seatIds, SeatBookingStatus.Booked);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Contains("Booked successful", result.Messages);
        }

        [Fact]
        public async Task CancelHoldByConnectionAsync_ResetsSeats_WhenSeatsExist()
        {
            // Arrange
            var connectionId = "conn123";
            var userId = Guid.NewGuid();
            var seats = new List<SeatSchedule>
            {
                new SeatSchedule
                {
                    Id = Guid.NewGuid(),
                    HoldByConnectionId = connectionId,
                    HoldByUserId = userId,
                    Status = SeatBookingStatus.Hold
                }
            };

            _mockSeatScheduleRepo.Setup(r => r.GetAllAsync(It.IsAny<Expression<Func<SeatSchedule, bool>>>()))
                .ReturnsAsync(seats);

            _mockUow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            await _service.CancelHoldByConnectionAsync(connectionId, userId);

            // Assert
            _mockUow.Verify(u => u.SaveChangesAsync(), Times.Once);
            Assert.Equal(SeatBookingStatus.Available, seats[0].Status);
        }

        [Fact]
        public async Task GetHoldSeatByUserIdAsync_ReturnsEmpty_WhenNoSeats()
        {
            // Arrange
            _mockSeatScheduleRepo.Setup(r => r.GetAllAsync(It.IsAny<Expression<Func<SeatSchedule, bool>>>()))
                .ReturnsAsync(new List<SeatSchedule>());

            // Act
            var result = await _service.GetHoldSeatByUserIdAsync(Guid.NewGuid(), Guid.NewGuid());

            // Assert
            Assert.Empty(result);
        }
        [Fact]
        public async Task HoldSeatAsync_ShouldHoldSeatsSuccessfully()
        {
            // Arrange
            var showtimeId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var connectionId = "conn1";
            var seatIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            var now = DateTime.UtcNow;

            var seats = seatIds.Select(id => new SeatSchedule
            {
                Id = id,
                ShowtimeId = showtimeId,
                Status = SeatBookingStatus.Available
            }).ToList();

            _mockSeatScheduleRepo.Setup(r => r.GetAllAsync(It.IsAny<Expression<Func<SeatSchedule, bool>>>()))
                .ReturnsAsync(seats);

            _mockUow.Setup(u => u.BeginTransactionAsync())
                .ReturnsAsync(Mock.Of<IDbContextTransaction>());

            _mockUow.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            _mockMapper.Setup(m => m.Map<List<SeatScheduleResponse>>(It.IsAny<List<SeatSchedule>>()))
                .Returns(seats.Select(s => new SeatScheduleResponse { Id = s.Id }).ToList());

            // Act
            var result = await _service.HoldSeatAsync(showtimeId, seatIds, userId, connectionId);

            // Assert
            Assert.NotEmpty(result);
            Assert.All(result, r => Assert.Contains(r.Id, seatIds));
        }
        [Fact]
        public async Task ConfirmSeatAsync_ShouldUpdateSeatsToBooked()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var seatIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

            var seats = seatIds.Select(id => new SeatSchedule
            {
                Id = id,
                Status = SeatBookingStatus.Hold,
                HoldByUserId = userId
            }).ToList();

            _mockSeatScheduleRepo.Setup(r => r.GetAllAsync(It.IsAny<Expression<Func<SeatSchedule, bool>>>()))
                .ReturnsAsync(seats);

            _mockUow.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _service.ConfirmSeatAsync(seatIds, userId);

            // Assert
            Assert.True(result.Succeeded);
            _mockUow.Verify(u => u.SaveChangesAsync(), Times.Once);
        }
        [Fact]
        public async Task CancelHoldAsync_ShouldReleaseHeldSeats()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var seatIds = new List<Guid> { Guid.NewGuid() };

            var seats = seatIds.Select(id => new SeatSchedule
            {
                Id = id,
                Status = SeatBookingStatus.Hold,
                HoldByUserId = userId
            }).ToList();

            _mockSeatScheduleRepo.Setup(r => r.GetAllAsync(It.IsAny<Expression<Func<SeatSchedule, bool>>>()))
                .ReturnsAsync(seats);

            _mockUow.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _service.CancelHoldAsync(seatIds, userId);

            // Assert
            Assert.True(result.Succeeded);
            Assert.All(seats, s => Assert.Equal(SeatBookingStatus.Available, s.Status));
        }
        [Fact]
        public async Task GetShowTimeBySeatScheduleAsync_ReturnsNull_WhenSeatScheduleNotFound()
        {
            // Arrange
            var id = Guid.NewGuid();
            _mockUow.Setup(u => u.SeatScheduleRepo.GetAsync(It.IsAny<Expression<Func<SeatSchedule, bool>>>()))
                .ReturnsAsync((SeatSchedule)null);

            // Act
            var result = await _service.GetShowTimeBySeatScheduleAsync(id);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetShowTimeBySeatScheduleAsync_ReturnsMappedResponse_WhenSeatScheduleFound()
        {
            // Arrange
            var id = Guid.NewGuid();
            var seatSchedule = new SeatSchedule { Id = id, /* other properties */ };
            var mappedResponse = new SeatScheduleResponse { Id = id, /* other properties */ };

            _mockUow.Setup(u => u.SeatScheduleRepo.GetAsync(It.IsAny<Expression<Func<SeatSchedule, bool>>>()))
                .ReturnsAsync(seatSchedule);
            _mockMapper.Setup(m => m.Map<SeatScheduleResponse>(seatSchedule)).Returns(mappedResponse);

            // Act
            var result = await _service.GetShowTimeBySeatScheduleAsync(id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(mappedResponse.Id, result.Id);
            // Add more assertions for other properties if needed
        }

    }
}
