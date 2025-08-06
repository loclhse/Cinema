using Application;
using Application.Common;
using Application.IRepos;
using Application.Services;
using Application.ViewModel;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ZTest.Services
{
    public class BackgroundServiceTests
    {
        private readonly Mock<IUnitOfWork> _uow;
        private readonly Mock<IAuthRepo> _authRepo;
        private readonly Mock<ISeatScheduleRepo> _seatScheduleRepo;
        private readonly Mock<ISubscriptionRepo> _subscriptionRepo;
        private readonly Mock<IUserRepo> _userRepo;
        private readonly BackgroundService _sut;

        public BackgroundServiceTests()
        {
            _uow = new Mock<IUnitOfWork>();
            _authRepo = new Mock<IAuthRepo>();
            _seatScheduleRepo = new Mock<ISeatScheduleRepo>();
            _subscriptionRepo = new Mock<ISubscriptionRepo>();
            _userRepo = new Mock<IUserRepo>();

            // Setup UoW to return the mocked repos
            _uow.SetupGet(u => u.SeatScheduleRepo).Returns(_seatScheduleRepo.Object);
            _uow.SetupGet(u => u.SubscriptionRepo).Returns(_subscriptionRepo.Object);
            _uow.SetupGet(u => u.UserRepo).Returns(_userRepo.Object);

            _sut = new BackgroundService(_uow.Object, _authRepo.Object);
        }

        [Fact]
        public async Task ChangeSeatBookingStatus_Should_Update_Expired_Seats()
        {
            // Arrange
            var currentTime = DateTime.UtcNow;
            var expiredSeats = new List<SeatSchedule>
            {
                new SeatSchedule
                {
                    Id = Guid.NewGuid(),
                    Status = SeatBookingStatus.Hold,
                    HoldUntil = currentTime.AddMinutes(-5)
                },
                new SeatSchedule
                {
                    Id = Guid.NewGuid(),
                    Status = SeatBookingStatus.Hold,
                    HoldUntil = currentTime.AddMinutes(-10)
                }
            };

            _seatScheduleRepo.Setup(s => s.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<Func<SeatSchedule, bool>>>()))
                .ReturnsAsync(expiredSeats);

            // Act
            await _sut.ChangeSeatBookingStatus();

            // Assert
            _uow.Verify(u => u.SaveChangesAsync(), Times.Once);
            Assert.All(expiredSeats, seat =>
            {
                Assert.Equal(SeatBookingStatus.Available, seat.Status);
                Assert.Null(seat.HoldUntil);
            });
        }

        [Fact]
        public async Task ChangeSeatBookingStatus_Should_Handle_No_Expired_Seats()
        {
            // Arrange
            var currentTime = DateTime.UtcNow;
            var activeSeats = new List<SeatSchedule>
            {
                new SeatSchedule
                {
                    Id = Guid.NewGuid(),
                    Status = SeatBookingStatus.Hold,
                    HoldUntil = currentTime.AddMinutes(5)
                }
            };

            _seatScheduleRepo.Setup(s => s.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<Func<SeatSchedule, bool>>>()))
                .ReturnsAsync(activeSeats);

            // Act
            await _sut.ChangeSeatBookingStatus();

            // Assert
            _uow.Verify(u => u.SaveChangesAsync(), Times.Once);
            Assert.All(activeSeats, seat =>
            {
                Assert.Equal(SeatBookingStatus.Hold, seat.Status);
                Assert.NotNull(seat.HoldUntil);
            });
        }

        [Fact]
        public async Task ChangeSeatBookingStatus_Should_Handle_Empty_Expired_Seats()
        {
            // Arrange
            _seatScheduleRepo.Setup(s => s.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<Func<SeatSchedule, bool>>>()))
                .ReturnsAsync(new List<SeatSchedule>());

            // Act
            await _sut.ChangeSeatBookingStatus();

            // Assert
            _uow.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task ChangeSeatBookingStatus_Should_Handle_DbUpdateConcurrencyException()
        {
            // Arrange
            var expiredSeats = new List<SeatSchedule>
            {
                new SeatSchedule
                {
                    Id = Guid.NewGuid(),
                    Status = SeatBookingStatus.Hold,
                    HoldUntil = DateTime.UtcNow.AddMinutes(-5)
                }
            };

            _seatScheduleRepo.Setup(s => s.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<Func<SeatSchedule, bool>>>()))
                .ReturnsAsync(expiredSeats);
            _uow.Setup(u => u.SaveChangesAsync())
                .ThrowsAsync(new DbUpdateConcurrencyException("Concurrency conflict"));

            // Act & Assert - Should not throw exception
            await _sut.ChangeSeatBookingStatus();
        }

        [Fact]
        public async Task IsSubscriptionExpired_Should_Update_Expired_Subscriptions()
        {
            // Arrange
            var currentTime = DateOnly.FromDateTime(DateTime.UtcNow);
            var userId = Guid.NewGuid();
            var expiredSubscriptions = new List<Subscription>
            {
                new Subscription
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Status = SubscriptionStatus.active,
                    EndDate = currentTime
                }
            };

            var user = new AppUser { Id = userId, FullName = "Test User" };

            _subscriptionRepo.Setup(s => s.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Subscription, bool>>>()))
                .ReturnsAsync(expiredSubscriptions);
            _userRepo.Setup(u => u.GetByIdAsync(userId))
                .ReturnsAsync(user);
            _authRepo.Setup(a => a.RemoveUserFromRoleAsync(userId, "Customer"))
                .ReturnsAsync(new OperationResult(new[] { "Role removed successfully" }, true));
            _authRepo.Setup(a => a.AddUserToRoleAsync(userId, "Member"))
                .ReturnsAsync(new OperationResult(new[] { "Role added successfully" }, true));

            // Act
            await _sut.IsSubscriptionExpired();

            // Assert
            _uow.Verify(u => u.SaveChangesAsync(), Times.Once);
            Assert.All(expiredSubscriptions, subscription =>
            {
                Assert.Equal(SubscriptionStatus.expired, subscription.Status);
            });
        }

        [Fact]
        public async Task IsSubscriptionExpired_Should_Handle_No_Expired_Subscriptions()
        {
            // Arrange
            var currentTime = DateOnly.FromDateTime(DateTime.UtcNow);
            var activeSubscriptions = new List<Subscription>
            {
                new Subscription
                {
                    Id = Guid.NewGuid(),
                    UserId = Guid.NewGuid(),
                    Status = SubscriptionStatus.active,
                    EndDate = currentTime.AddDays(1)
                }
            };

            _subscriptionRepo.Setup(s => s.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Subscription, bool>>>()))
                .ReturnsAsync(activeSubscriptions);

            // Act
            await _sut.IsSubscriptionExpired();

            // Assert
            _uow.Verify(u => u.SaveChangesAsync(), Times.Once);
            Assert.All(activeSubscriptions, subscription =>
            {
                Assert.Equal(SubscriptionStatus.active, subscription.Status);
            });
        }

        [Fact]
        public async Task IsSubscriptionExpired_Should_Handle_Empty_Expired_Subscriptions()
        {
            // Arrange
            _subscriptionRepo.Setup(s => s.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Subscription, bool>>>()))
                .ReturnsAsync(new List<Subscription>());

            // Act
            await _sut.IsSubscriptionExpired();

            // Assert
            _uow.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task IsSubscriptionExpired_Should_Handle_User_Not_Found()
        {
            // Arrange
            var currentTime = DateOnly.FromDateTime(DateTime.UtcNow);
            var userId = Guid.NewGuid();
            var expiredSubscriptions = new List<Subscription>
            {
                new Subscription
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Status = SubscriptionStatus.active,
                    EndDate = currentTime
                }
            };

            _subscriptionRepo.Setup(s => s.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Subscription, bool>>>()))
                .ReturnsAsync(expiredSubscriptions);
            _userRepo.Setup(u => u.GetByIdAsync(userId))
                .ReturnsAsync((AppUser)null);

            // Act
            await _sut.IsSubscriptionExpired();

            // Assert
            _uow.Verify(u => u.SaveChangesAsync(), Times.Once);
            Assert.All(expiredSubscriptions, subscription =>
            {
                Assert.Equal(SubscriptionStatus.expired, subscription.Status);
            });
        }

        [Fact]
        public async Task IsSubscriptionExpired_Should_Handle_Role_Assignment_Failure()
        {
            // Arrange
            var currentTime = DateOnly.FromDateTime(DateTime.UtcNow);
            var userId = Guid.NewGuid();
            var expiredSubscriptions = new List<Subscription>
            {
                new Subscription
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Status = SubscriptionStatus.active,
                    EndDate = currentTime
                }
            };

            var user = new AppUser { Id = userId, FullName = "Test User" };

            _subscriptionRepo.Setup(s => s.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Subscription, bool>>>()))
                .ReturnsAsync(expiredSubscriptions);
            _userRepo.Setup(u => u.GetByIdAsync(userId))
                .ReturnsAsync(user);
            _authRepo.Setup(a => a.RemoveUserFromRoleAsync(userId, "Customer"))
                .ReturnsAsync(new OperationResult(new[] { "Role removed successfully" }, true));
            _authRepo.Setup(a => a.AddUserToRoleAsync(userId, "Member"))
                .ReturnsAsync(new OperationResult(new[] { "Failed to assign role" }));

            // Act
            await _sut.IsSubscriptionExpired();

            // Assert
            _uow.Verify(u => u.SaveChangesAsync(), Times.Once);
            Assert.All(expiredSubscriptions, subscription =>
            {
                Assert.Equal(SubscriptionStatus.expired, subscription.Status);
            });
        }

        [Fact]
        public async Task IsSubscriptionExpired_Should_Handle_DbUpdateConcurrencyException()
        {
            // Arrange
            var currentTime = DateOnly.FromDateTime(DateTime.UtcNow);
            var userId = Guid.NewGuid();
            var expiredSubscriptions = new List<Subscription>
            {
                new Subscription
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Status = SubscriptionStatus.active,
                    EndDate = currentTime
                }
            };

            var user = new AppUser { Id = userId, FullName = "Test User" };

            _subscriptionRepo.Setup(s => s.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Subscription, bool>>>()))
                .ReturnsAsync(expiredSubscriptions);
            _userRepo.Setup(u => u.GetByIdAsync(userId))
                .ReturnsAsync(user);
            _authRepo.Setup(a => a.RemoveUserFromRoleAsync(userId, "Customer"))
                .ReturnsAsync(new OperationResult(new[] { "Role removed successfully" }, true));
            _authRepo.Setup(a => a.AddUserToRoleAsync(userId, "Member"))
                .ReturnsAsync(new OperationResult(new[] { "Role added successfully" }, true));
            _uow.Setup(u => u.SaveChangesAsync())
                .ThrowsAsync(new DbUpdateConcurrencyException("Concurrency conflict"));

            // Act & Assert - Should not throw exception
            await _sut.IsSubscriptionExpired();
        }

        [Fact]
        public async Task IsSubscriptionExpired_Should_Handle_Multiple_Expired_Subscriptions()
        {
            // Arrange
            var currentTime = DateOnly.FromDateTime(DateTime.UtcNow);
            var userId1 = Guid.NewGuid();
            var userId2 = Guid.NewGuid();
            var expiredSubscriptions = new List<Subscription>
            {
                new Subscription
                {
                    Id = Guid.NewGuid(),
                    UserId = userId1,
                    Status = SubscriptionStatus.active,
                    EndDate = currentTime
                },
                new Subscription
                {
                    Id = Guid.NewGuid(),
                    UserId = userId2,
                    Status = SubscriptionStatus.active,
                    EndDate = currentTime
                }
            };

            var user1 = new AppUser { Id = userId1, FullName = "User 1" };
            var user2 = new AppUser { Id = userId2, FullName = "User 2" };

            _subscriptionRepo.Setup(s => s.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Subscription, bool>>>()))
                .ReturnsAsync(expiredSubscriptions);
            _userRepo.Setup(u => u.GetByIdAsync(userId1))
                .ReturnsAsync(user1);
            _userRepo.Setup(u => u.GetByIdAsync(userId2))
                .ReturnsAsync(user2);
            _authRepo.Setup(a => a.RemoveUserFromRoleAsync(It.IsAny<Guid>(), "Customer"))
                .ReturnsAsync(new OperationResult(new[] { "Role removed successfully" }, true));
            _authRepo.Setup(a => a.AddUserToRoleAsync(It.IsAny<Guid>(), "Member"))
                .ReturnsAsync(new OperationResult(new[] { "Role added successfully" }, true));

            // Act
            await _sut.IsSubscriptionExpired();

            // Assert
            _uow.Verify(u => u.SaveChangesAsync(), Times.Once);
            Assert.All(expiredSubscriptions, subscription =>
            {
                Assert.Equal(SubscriptionStatus.expired, subscription.Status);
            });
        }
    }
}
