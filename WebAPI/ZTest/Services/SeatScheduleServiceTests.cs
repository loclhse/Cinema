using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Services;
using Application.IServices;
using Application.ViewModel.Response;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Application.IRepos;
using Application.Common;
using Application;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

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
            // Arrange
            var showtimeId = Guid.NewGuid();
            var seats = new List<SeatSchedule> { new SeatSchedule { Id = Guid.NewGuid(), ShowtimeId = showtimeId } };
            var mapped = new List<SeatScheduleResponse> { new SeatScheduleResponse { Id = seats[0].Id } };

            _mockSeatScheduleRepo.Setup(r => r.GetAllAsync(
                It.IsAny<Expression<Func<SeatSchedule, bool>>>())
            ).ReturnsAsync(seats);
            _mockMapper.Setup(m => m.Map<IEnumerable<SeatScheduleResponse>>(seats)).Returns(mapped);

            // Act
            var result = await _service.GetSeatSchedulesByShowtimeAsync(showtimeId);

            // Assert
            Assert.NotNull(result);
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
    }
}
