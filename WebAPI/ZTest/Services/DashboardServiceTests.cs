using Application;
using Application.IServices;
using Application.Services;
using Application.ViewModel;
using Application.ViewModel.Response;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Migrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace ZTest.Services
{
    public class DashboardServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly DashboardService _dashboardService;

        public DashboardServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapperMock = new Mock<IMapper>();
            _dashboardService = new DashboardService(_unitOfWorkMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task GetMovieRankingsDataAsync_ShouldReturnMovieRankings()
        {
            // Arrange
            var payments = new List<Payment>
            {
                new Payment { AmountPaid = 100, OrderId = Guid.Parse("f2d4169e-3b65-4c20-a961-82e9bc441149"), Status = PaymentStatus.Success, IsDeleted = false },
                new Payment { AmountPaid = 200, OrderId = Guid.Parse("dc781fd8-f85a-4479-bdb0-646b42641027"), Status = PaymentStatus.Success, IsDeleted = false }
            };

            var seatSchedules = new List<SeatSchedule>
            {
                new SeatSchedule { OrderId = Guid.Parse("f2d4169e-3b65-4c20-a961-82e9bc441149"), Showtime = new Showtime { Movie = new Movie { Id = Guid.Parse("07f696f6-7981-4c1f-865e-a7289f5c6d91"), Title = "Movie A", Img = "posterA.jpg", IsDeleted = false } } },
                new SeatSchedule { OrderId = Guid.Parse("dc781fd8-f85a-4479-bdb0-646b42641027"), Showtime = new Showtime { Movie = new Movie { Id = Guid.Parse("e869982f-fa2c-4c8f-98da-90d93148e429"), Title = "Movie B", Img = "posterB.jpg", IsDeleted = false } } }
            };

            // Mocking repository methods
            _unitOfWorkMock.Setup(u => u.PaymentRepo.GetAllAsync(It.IsAny<Expression<Func<Payment, bool>>>(), It.IsAny<Func<IQueryable<Payment>, IIncludableQueryable<Payment, object>>>()))
                .ReturnsAsync(payments);

            _unitOfWorkMock.Setup(u => u.SeatScheduleRepo.GetAllAsync(It.IsAny<Expression<Func<SeatSchedule, bool>>>(), It.IsAny<Func<IQueryable<SeatSchedule>, IIncludableQueryable<SeatSchedule, object>>>()))
                .ReturnsAsync(seatSchedules);

            // Act
            var result = await _dashboardService.GetMovieRankingsDataAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("Movie B", result[0].MovieName); // Movie with higher revenue first
            Assert.Equal(200, result[0].TotalRevenue);
            Assert.Equal(1, result[0].Rank); // Should be ranked #1
        }

        [Fact]
        public async Task GetMovieRankingsDataAsync_ShouldReturnEmptyList_WhenNoSuccessfulPayments()
        {
            // Arrange
            var payments = new List<Payment>();
            var seatSchedules = new List<SeatSchedule>();

            _unitOfWorkMock.Setup(u => u.PaymentRepo.GetAllAsync(It.IsAny<Expression<Func<Payment, bool>>>(), It.IsAny<Func<IQueryable<Payment>, IIncludableQueryable<Payment, object>>>()))
                .ReturnsAsync(payments);

            _unitOfWorkMock.Setup(u => u.SeatScheduleRepo.GetAllAsync(It.IsAny<Expression<Func<SeatSchedule, bool>>>(), It.IsAny<Func<IQueryable<SeatSchedule>, IIncludableQueryable<SeatSchedule, object>>>()))
                .ReturnsAsync(seatSchedules);

            // Act
            var result = await _dashboardService.GetMovieRankingsDataAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetMovieRankingsDataAsync_ShouldHandleNullMovieData()
        {
            // Arrange
            var payments = new List<Payment>
            {
                new Payment { AmountPaid = 100, OrderId = Guid.NewGuid(), Status = PaymentStatus.Success, IsDeleted = false }
            };

            var seatSchedules = new List<SeatSchedule>
            {
                new SeatSchedule 
                { 
                    OrderId = payments[0].OrderId, 
                    Showtime = new Showtime { Movie = null }
                }
            };

            _unitOfWorkMock.Setup(u => u.PaymentRepo.GetAllAsync(It.IsAny<Expression<Func<Payment, bool>>>(), It.IsAny<Func<IQueryable<Payment>, IIncludableQueryable<Payment, object>>>()))
                .ReturnsAsync(payments);

            _unitOfWorkMock.Setup(u => u.SeatScheduleRepo.GetAllAsync(It.IsAny<Expression<Func<SeatSchedule, bool>>>(), It.IsAny<Func<IQueryable<SeatSchedule>, IIncludableQueryable<SeatSchedule, object>>>()))
                .ReturnsAsync(seatSchedules);

            // Act
            var result = await _dashboardService.GetMovieRankingsDataAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetMovieRankingsDataAsync_ShouldHandleDeletedMovies()
        {
            // Arrange
            var payments = new List<Payment>
            {
                new Payment { AmountPaid = 100, OrderId = Guid.NewGuid(), Status = PaymentStatus.Success, IsDeleted = false }
            };

            var seatSchedules = new List<SeatSchedule>
            {
                new SeatSchedule 
                { 
                    OrderId = payments[0].OrderId, 
                    Showtime = new Showtime { Movie = new Movie { Id = Guid.NewGuid(), Title = "Deleted Movie", IsDeleted = true } }
                }
            };

            _unitOfWorkMock.Setup(u => u.PaymentRepo.GetAllAsync(It.IsAny<Expression<Func<Payment, bool>>>(), It.IsAny<Func<IQueryable<Payment>, IIncludableQueryable<Payment, object>>>()))
                .ReturnsAsync(payments);

            _unitOfWorkMock.Setup(u => u.SeatScheduleRepo.GetAllAsync(It.IsAny<Expression<Func<SeatSchedule, bool>>>(), It.IsAny<Func<IQueryable<SeatSchedule>, IIncludableQueryable<SeatSchedule, object>>>()))
                .ReturnsAsync(seatSchedules);

            // Act
            var result = await _dashboardService.GetMovieRankingsDataAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetMovieRankingsDataAsync_ShouldCalculateAverageTicketPrice()
        {
            // Arrange
            var orderId1 = Guid.NewGuid();
            var orderId2 = Guid.NewGuid();
            var payments = new List<Payment>
            {
                new Payment { AmountPaid = 100, OrderId = orderId1, Status = PaymentStatus.Success, IsDeleted = false },
                new Payment { AmountPaid = 200, OrderId = orderId2, Status = PaymentStatus.Success, IsDeleted = false }
            };

            var seatSchedules = new List<SeatSchedule>
            {
                new SeatSchedule 
                { 
                    OrderId = orderId1, 
                    Showtime = new Showtime { Movie = new Movie { Id = Guid.NewGuid(), Title = "Test Movie", Img = "test.jpg", IsDeleted = false } } 
                },
                new SeatSchedule 
                { 
                    OrderId = orderId2, 
                    Showtime = new Showtime { Movie = new Movie { Id = Guid.NewGuid(), Title = "Test Movie", Img = "test.jpg", IsDeleted = false } } 
                }
            };

            _unitOfWorkMock.Setup(u => u.PaymentRepo.GetAllAsync(It.IsAny<Expression<Func<Payment, bool>>>(), It.IsAny<Func<IQueryable<Payment>, IIncludableQueryable<Payment, object>>>()))
                .ReturnsAsync(payments);

            _unitOfWorkMock.Setup(u => u.SeatScheduleRepo.GetAllAsync(It.IsAny<Expression<Func<SeatSchedule, bool>>>(), It.IsAny<Func<IQueryable<SeatSchedule>, IIncludableQueryable<SeatSchedule, object>>>()))
                .ReturnsAsync(seatSchedules);

            // Act
            var result = await _dashboardService.GetMovieRankingsDataAsync();

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            var ranking = result.First();
            Assert.Equal(150, ranking.AverageTicketPrice); // (100 + 200) / 2
        }

        [Fact]
        public async Task GetMovieRankingsDataAsync_ShouldRespectLimit()
        {
            // Arrange
            var limit = 1;
            var payments = new List<Payment>
            {
                new Payment { AmountPaid = 100, OrderId = Guid.NewGuid(), Status = PaymentStatus.Success, IsDeleted = false },
                new Payment { AmountPaid = 200, OrderId = Guid.NewGuid(), Status = PaymentStatus.Success, IsDeleted = false },
                new Payment { AmountPaid = 300, OrderId = Guid.NewGuid(), Status = PaymentStatus.Success, IsDeleted = false }
            };

            var seatSchedules = new List<SeatSchedule>
            {
                new SeatSchedule { OrderId = payments[0].OrderId, Showtime = new Showtime { Movie = new Movie { Id = Guid.NewGuid(), Title = "Movie A", Img = "posterA.jpg", IsDeleted = false } } },
                new SeatSchedule { OrderId = payments[1].OrderId, Showtime = new Showtime { Movie = new Movie { Id = Guid.NewGuid(), Title = "Movie B", Img = "posterB.jpg", IsDeleted = false } } },
                new SeatSchedule { OrderId = payments[2].OrderId, Showtime = new Showtime { Movie = new Movie { Id = Guid.NewGuid(), Title = "Movie C", Img = "posterC.jpg", IsDeleted = false } } }
            };

            _unitOfWorkMock.Setup(u => u.PaymentRepo.GetAllAsync(It.IsAny<Expression<Func<Payment, bool>>>(), It.IsAny<Func<IQueryable<Payment>, IIncludableQueryable<Payment, object>>>()))
                .ReturnsAsync(payments);

            _unitOfWorkMock.Setup(u => u.SeatScheduleRepo.GetAllAsync(It.IsAny<Expression<Func<SeatSchedule, bool>>>(), It.IsAny<Func<IQueryable<SeatSchedule>, IIncludableQueryable<SeatSchedule, object>>>()))
                .ReturnsAsync(seatSchedules);

            // Act
            var result = await _dashboardService.GetMovieRankingsDataAsync(limit);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(limit, result.Count);
            Assert.Equal("Movie C", result[0].MovieName); // Highest revenue movie
            Assert.Equal(300, result[0].TotalRevenue);
        }

        [Fact]
        public async Task GetMovieRankingsAsync_ShouldReturnApiResp()
        {
            // Arrange
            var payments = new List<Payment>
            {
                new Payment { AmountPaid = 100, OrderId = Guid.NewGuid(), Status = PaymentStatus.Success, IsDeleted = false }
            };

            var seatSchedules = new List<SeatSchedule>
            {
                new SeatSchedule { OrderId = payments[0].OrderId, Showtime = new Showtime { Movie = new Movie { Id = Guid.NewGuid(), Title = "Test Movie", Img = "test.jpg", IsDeleted = false } } }
            };

            _unitOfWorkMock.Setup(u => u.PaymentRepo.GetAllAsync(It.IsAny<Expression<Func<Payment, bool>>>(), It.IsAny<Func<IQueryable<Payment>, IIncludableQueryable<Payment, object>>>()))
                .ReturnsAsync(payments);

            _unitOfWorkMock.Setup(u => u.SeatScheduleRepo.GetAllAsync(It.IsAny<Expression<Func<SeatSchedule, bool>>>(), It.IsAny<Func<IQueryable<SeatSchedule>, IIncludableQueryable<SeatSchedule, object>>>()))
                .ReturnsAsync(seatSchedules);

            // Act
            var result = await _dashboardService.GetMovieRankingsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ApiResp>(result);
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Result);
        }

        [Fact]
        public async Task GetMovieRankingsAsync_ShouldReturnBadRequest_OnException()
        {
            // Arrange
            _unitOfWorkMock.Setup(u => u.PaymentRepo.GetAllAsync(It.IsAny<Expression<Func<Payment, bool>>>(), It.IsAny<Func<IQueryable<Payment>, IIncludableQueryable<Payment, object>>>()))
                .Throws(new Exception("Test exception"));

            // Act
            var result = await _dashboardService.GetMovieRankingsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ApiResp>(result);
            Assert.False(result.IsSuccess);
            Assert.Equal("Error getting movie rankings: Test exception", result.ErrorMessage);
        }
    }
}