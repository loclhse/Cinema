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
        public async Task GetDashboardDataAsync_ShouldReturnDashboardData()
        {
            // Arrange
            _unitOfWorkMock.Setup(u => u.PaymentRepo.GetAllAsync(It.IsAny<Expression<Func<Payment, bool>>>()))
                .ReturnsAsync(new List<Payment>());
            _unitOfWorkMock.Setup(u => u.SeatScheduleRepo.GetAllAsync(It.IsAny<Expression<Func<SeatSchedule, bool>>>()))
                .ReturnsAsync(new List<SeatSchedule>());
            _unitOfWorkMock.Setup(u => u.OrderRepo.GetAllAsync(It.IsAny<Expression<Func<Order, bool>>>()))
                .ReturnsAsync(new List<Order>());

            // Act
            var result = await _dashboardService.GetDashboardDataAsync();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ApiResp>(result);
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Result);
        }

        [Fact]
        public async Task GetDashboardDataAsync_ShouldReturnBadRequest_OnException()
        {
            // Arrange
            _unitOfWorkMock.Setup(u => u.PaymentRepo.GetAllAsync(It.IsAny<Expression<Func<Payment, bool>>>()))
                .Throws(new Exception("Test exception"));

            // Act
            var result = await _dashboardService.GetDashboardDataAsync();

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Error getting dashboard data: Test exception", result.ErrorMessage);
        }

        [Fact]
        public async Task GetRevenueAnalyticsAsync_ShouldReturnRevenueAnalytics()
        {
            // Arrange
            var startDate = DateTime.UtcNow.AddDays(-30);
            var endDate = DateTime.UtcNow;
            _unitOfWorkMock.Setup(u => u.PaymentRepo.GetAllAsync(It.IsAny<Expression<Func<Payment, bool>>>()))
                .ReturnsAsync(new List<Payment>());

            // Act
            var result = await _dashboardService.GetRevenueAnalyticsAsync(startDate, endDate);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ApiResp>(result);
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Result);
        }

        [Fact]
        public async Task GetRevenueAnalyticsAsync_ShouldReturnBadRequest_OnException()
        {
            // Arrange
            _unitOfWorkMock.Setup(u => u.PaymentRepo.GetAllAsync(It.IsAny<Expression<Func<Payment, bool>>>()))
                .Throws(new Exception("Test exception"));

            // Act
            var result = await _dashboardService.GetRevenueAnalyticsAsync();

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Error getting revenue analytics: Test exception", result.ErrorMessage);
        }

        [Fact]
        public async Task GetMovieRankingsAsync_ShouldReturnMovieRankings()
        {
            // Arrange
            _unitOfWorkMock.Setup(u => u.SeatScheduleRepo.GetAllAsync(It.IsAny<Expression<Func<SeatSchedule, bool>>>()))
                .ReturnsAsync(new List<SeatSchedule>());

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
            _unitOfWorkMock.Setup(u => u.SeatScheduleRepo.GetAllAsync(It.IsAny<Expression<Func<SeatSchedule, bool>>>()))
                .Throws(new Exception("Test exception"));

            // Act
            var result = await _dashboardService.GetMovieRankingsAsync();

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Error getting movie rankings: Test exception", result.ErrorMessage);
        }

        [Fact]
        public async Task GetDailyRevenueAsync_ShouldReturnDailyRevenue()
        {
            // Arrange
            var date = DateTime.UtcNow;
            _unitOfWorkMock.Setup(u => u.PaymentRepo.GetAllAsync(It.IsAny<Expression<Func<Payment, bool>>>()))
                .ReturnsAsync(new List<Payment>());

            // Act
            var result = await _dashboardService.GetDailyRevenueAsync(date);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ApiResp>(result);
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Result);
        }

        [Fact]
        public async Task GetDailyRevenueAsync_ShouldReturnBadRequest_OnException()
        {
            // Arrange
            var date = DateTime.UtcNow;
            _unitOfWorkMock.Setup(u => _dashboardService.GetRevenueForDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Throws(new Exception("Test exception"));

            // Act
            var result = await _dashboardService.GetDailyRevenueAsync(date);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Error getting daily revenue: Test exception", result.ErrorMessage);
        }

        [Fact]
        public async Task GetWeeklyRevenueAsync_ShouldReturnWeeklyRevenue()
        {
            // Arrange
            var weekStart = DateTime.UtcNow;
            _unitOfWorkMock.Setup(u => u.PaymentRepo.GetAllAsync(It.IsAny<Expression<Func<Payment, bool>>>()))
                .ReturnsAsync(new List<Payment>());

            // Act
            var result = await _dashboardService.GetWeeklyRevenueAsync(weekStart);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ApiResp>(result);
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Result);
        }

        [Fact]
        public async Task GetWeeklyRevenueAsync_ShouldReturnBadRequest_OnException()
        {
            // Arrange
            var weekStart = DateTime.UtcNow;
            _unitOfWorkMock.Setup(u => _dashboardService.GetRevenueForDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Throws(new Exception("Test exception"));

            // Act
            var result = await _dashboardService.GetWeeklyRevenueAsync(weekStart);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Error getting weekly revenue: Test exception", result.ErrorMessage);
        }

        [Fact]
        public async Task GetMonthlyRevenueAsync_ShouldReturnMonthlyRevenue()
        {
            // Arrange
            var year = 2023;
            var month = 1;

            // Act
            var result = await _dashboardService.GetMonthlyRevenueAsync(year, month);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ApiResp>(result);
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Result);
        }

        [Fact]
        public async Task GetMonthlyRevenueAsync_ShouldReturnBadRequest_OnException()
        {
            // Arrange
            var year = 2023;
            var month = 1;
            _unitOfWorkMock.Setup(u => _dashboardService.GetRevenueForDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Throws(new Exception("Test exception"));

            // Act
            var result = await _dashboardService.GetMonthlyRevenueAsync(year, month);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Error getting monthly revenue: Test exception", result.ErrorMessage);
        }
        [Fact]
        public async Task GetWeeklyRevenuesAsync_ShouldReturnWeeklyRevenues()
        {
            // Arrange
            var startDate = new DateTime(2023, 1, 1);
            var endDate = new DateTime(2023, 1, 31);

            // Mocking GetRevenueForDateRangeAsync and GetTicketCountForDateRangeAsync
            _unitOfWorkMock.Setup(u => u.PaymentRepo.GetAllAsync(It.IsAny<Expression<Func<Payment, bool>>>()))
                .ReturnsAsync(new List<Payment>
                {
            new Payment { AmountPaid = 100, PaymentTime = startDate.AddDays(1) },
            new Payment { AmountPaid = 200, PaymentTime = startDate.AddDays(8) }
            // Thêm các payment khác nếu cần
                });

            _unitOfWorkMock.Setup(u => u.OrderRepo.GetAllAsync(It.IsAny<Expression<Func<Order, bool>>>()))
                .ReturnsAsync(new List<Order>
                {
            new Order { SeatSchedules = new List<SeatSchedule> { new SeatSchedule() } }
            // Thêm các order khác nếu cần
                });

            // Act
            var result = await _dashboardService.GetWeeklyRevenueAsync(startDate);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ApiResp>(result);
            Assert.True(result.IsSuccess);
            var weeklyRevenue = result.Result as WeeklyRevenue;
            Assert.NotNull(weeklyRevenue);
            Assert.Equal(300, weeklyRevenue.Revenue); // Tổng doanh thu cho 2 tuần
        }

        [Fact]
        public async Task GetWeeklyRevenuesAsync_ShouldReturnBadRequest_OnException()
        {
            // Arrange
            var weekStart = DateTime.UtcNow;
            _unitOfWorkMock.Setup(u => _dashboardService.GetRevenueForDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Throws(new Exception("Test exception"));

            // Act
            var result = await _dashboardService.GetWeeklyRevenueAsync(weekStart);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Error getting weekly revenue: Test exception", result.ErrorMessage);
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
            _unitOfWorkMock.Setup(u => u.PaymentRepo.GetAllAsync(It.IsAny<Expression<Func<Payment, bool>>>()))
                .ReturnsAsync(payments);

            _unitOfWorkMock.Setup(u => u.SeatScheduleRepo.GetAllAsync(It.IsAny<Expression<Func<SeatSchedule, bool>>>()))
                .ReturnsAsync(seatSchedules);

            // Act
            var result = await _dashboardService.GetMovieRankingsDataAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count); // Kiểm tra số lượng phim trong kết quả
            Assert.Equal("Movie B", result[0].MovieName); // Phim có doanh thu cao nhất
            Assert.Equal(200, result[0].TotalRevenue); // Doanh thu của phim
        }

        [Fact]
        public async Task GetMovieRankingsDataAsync_ShouldReturnEmptyList_WhenNoSuccessfulPayments()
        {
            // Arrange
            var payments = new List<Payment>(); // Không có payment nào
            var seatSchedules = new List<SeatSchedule>(); // Không có seat schedule nào

            // Mocking repository methods
            _unitOfWorkMock.Setup(u => u.PaymentRepo.GetAllAsync(It.IsAny<Expression<Func<Payment, bool>>>()))
                .ReturnsAsync(payments);

            _unitOfWorkMock.Setup(u => u.SeatScheduleRepo.GetAllAsync(It.IsAny<Expression<Func<SeatSchedule, bool>>>()))
                .ReturnsAsync(seatSchedules);

            // Act
            var result = await _dashboardService.GetMovieRankingsDataAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result); // Kết quả nên là một danh sách rỗng
        }

        [Fact]
        public async Task GetMovieRankingsDataAsync_ShouldReturnBadRequest_OnException()
        {
            // Arrange
            _unitOfWorkMock.Setup(u => u.PaymentRepo.GetAllAsync(It.IsAny<Expression<Func<Payment, bool>>>()))
                .Throws(new Exception("Test exception"));

            // Act
            var result = await _dashboardService.GetMovieRankingsDataAsync();

            // Assert
            Assert.Null(result); // Kết quả trả về không nên có
        }
        [Fact]
        public async Task GetMonthlyRevenuesAsync_ShouldReturnEmptyList_WhenNoRevenues()
        {
            // Arrange
            var startDate = new DateTime(2023, 1, 1);
            var endDate = new DateTime(2023, 12, 31);

            // Mocking GetRevenueForDateRangeAsync và GetTicketCountForDateRangeAsync trả về 0
            _unitOfWorkMock.Setup(u => _dashboardService.GetRevenueForDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(0);
            _unitOfWorkMock.Setup(u => _dashboardService.GetTicketCountForDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(0);

            // Act
            var result = await _dashboardService.GetMonthlyRevenueAsync(startDate.Year, startDate.Month);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ApiResp>(result);
            Assert.True(result.IsSuccess);
            var monthlyRevenues = result.Result as List<MonthlyRevenue>;
            Assert.NotEmpty(monthlyRevenues);
            Assert.All(monthlyRevenues, mr => Assert.Equal(0, mr.Revenue)); // Kiểm tra doanh thu
        }

        [Fact]
        public async Task GetMonthlyRevenuesAsync_ShouldReturnBadRequest_OnException()
        {
            // Arrange
            var year = 2023;
            var month = 1;
            _unitOfWorkMock.Setup(u => _dashboardService.GetRevenueForDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Throws(new Exception("Test exception"));

            // Act
            var result = await _dashboardService.GetMonthlyRevenueAsync(year, month);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Error getting monthly revenue: Test exception", result.ErrorMessage);
        }
    }
}