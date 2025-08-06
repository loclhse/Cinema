using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Application.Services;
using Application.IServices;
using Domain.Enums;
using Domain.Entities;
using Application.IRepos;
using Application;

namespace ZTest.Services
{
    public class BackgroundServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ISeatScheduleRepo> _seatScheduleRepoMock;
        private readonly Mock<ISubscriptionRepo> _subscriptionRepoMock;
        private readonly Mock<IAuthRepo> _authRepoMock;
        private readonly BackgroundService _backgroundService;

        public BackgroundServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _seatScheduleRepoMock = new Mock<ISeatScheduleRepo>();
            _subscriptionRepoMock = new Mock<ISubscriptionRepo>();

            // Thiết lập IUnitOfWork trả về đúng repo đã mock
            _unitOfWorkMock.SetupGet(u => u.SeatScheduleRepo).Returns(_seatScheduleRepoMock.Object);
            _unitOfWorkMock.SetupGet(u => u.SubscriptionRepo).Returns(_subscriptionRepoMock.Object);

            _backgroundService = new BackgroundService(_unitOfWorkMock.Object, _authRepoMock.Object);
        }

        [Fact]
        public async Task ChangeSeatBookingStatus_ShouldReleaseExpiredSeats()
        {
            // Arrange: 1 ghế hold đã quá hạn
            var expiredSeat = new SeatSchedule
            {
                Id = new Guid(),
                Status = SeatBookingStatus.Hold,
                HoldUntil = DateTime.UtcNow.AddMinutes(-10)
            };
            _seatScheduleRepoMock
                .Setup(r => r.GetAllAsync(It.IsAny<Expression<Func<SeatSchedule, bool>>>()))
                .ReturnsAsync(new List<SeatSchedule> { expiredSeat });

            // Act
            await _backgroundService.ChangeSeatBookingStatus();

            // Assert
            Assert.Equal(SeatBookingStatus.Available, expiredSeat.Status);
            Assert.Null(expiredSeat.HoldUntil);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task ChangeSeatBookingStatus_NoExpiredSeats_ShouldStillCallSave()
        {
            // Arrange: không có ghế hết hạn
            _seatScheduleRepoMock
                .Setup(r => r.GetAllAsync(It.IsAny<Expression<Func<SeatSchedule, bool>>>()))
                .ReturnsAsync(new List<SeatSchedule>());

            // Act
            await _backgroundService.ChangeSeatBookingStatus();

            // Assert
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task IsSubscriptionExpired_ShouldExpireTodaySubscriptions()
        {
            // Arrange: 1 subscription active hết hạn hôm nay
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var sub = new Subscription
            {
                Id = new Guid(),
                Status = SubscriptionStatus.active,
                EndDate = today
            };
            _subscriptionRepoMock
                .Setup(r => r.GetAllAsync(It.IsAny<Expression<Func<Subscription, bool>>>()))
                .ReturnsAsync(new List<Subscription> { sub });

            // Act
            await _backgroundService.IsSubscriptionExpired();

            // Assert
            Assert.Equal(SubscriptionStatus.expired, sub.Status);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task IsSubscriptionExpired_NoEndingToday_ShouldStillCallSave()
        {
            // Arrange: không có subscription hết hạn hôm nay
            _subscriptionRepoMock
                .Setup(r => r.GetAllAsync(It.IsAny<Expression<Func<Subscription, bool>>>()))
                .ReturnsAsync(new List<Subscription>());

            // Act
            await _backgroundService.IsSubscriptionExpired();

            // Assert
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }
    }
}
