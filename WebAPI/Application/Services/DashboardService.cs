using Application.IServices;
using Application.ViewModel;
using Application.ViewModel.Response;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public DashboardService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<ApiResp> GetDashboardDataAsync()
        {
            try
            {
                var dashboardData = new DashboardResponse();

                // Get revenue analytics
                var revenueAnalytics = await GetRevenueAnalyticsDataAsync();
                dashboardData.RevenueAnalytics = revenueAnalytics;

                // Get movie rankings
                var movieRankings = await GetMovieRankingsDataAsync();
                dashboardData.MovieRankings = movieRankings;

                return new ApiResp().SetOk(dashboardData);
            }
            catch (Exception ex)
            {
                return new ApiResp().SetBadRequest(null, $"Error getting dashboard data: {ex.Message}");
            }
        }

        public async Task<ApiResp> GetRevenueAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var revenueAnalytics = await GetRevenueAnalyticsDataAsync(startDate, endDate);
                return new ApiResp().SetOk(revenueAnalytics);
            }
            catch (Exception ex)
            {
                return new ApiResp().SetBadRequest(null, $"Error getting revenue analytics: {ex.Message}");
            }
        }

        public async Task<ApiResp> GetMovieRankingsAsync(int? limit = null)
        {
            try
            {
                var movieRankings = await GetMovieRankingsDataAsync(limit);
                return new ApiResp().SetOk(movieRankings);
            }
            catch (Exception ex)
            {
                return new ApiResp().SetBadRequest(null, $"Error getting movie rankings: {ex.Message}");
            }
        }

        public async Task<ApiResp> GetDailyRevenueAsync(DateTime date)
        {
            try
            {
                // Ensure UTC DateTime
                var utcDate = DateTime.SpecifyKind(date, DateTimeKind.Utc);
                var startOfDay = utcDate.Date;
                var endOfDay = startOfDay.AddDays(1).AddSeconds(-1);

                var dailyRevenue = await GetRevenueForDateRangeAsync(startOfDay, endOfDay);
                var ticketCount = await GetTicketCountForDateRangeAsync(startOfDay, endOfDay);

                var result = new DailyRevenue
                {
                    Date = date,
                    Revenue = dailyRevenue,
                    TicketCount = ticketCount
                };

                return new ApiResp().SetOk(result);
            }
            catch (Exception ex)
            {
                return new ApiResp().SetBadRequest(null, $"Error getting daily revenue: {ex.Message}");
            }
        }

        public async Task<ApiResp> GetWeeklyRevenueAsync(DateTime weekStart)
        {
            try
            {
                // Ensure UTC DateTime
                var utcWeekStart = DateTime.SpecifyKind(weekStart, DateTimeKind.Utc);
                var endOfWeek = utcWeekStart.AddDays(7).AddSeconds(-1);

                var weeklyRevenue = await GetRevenueForDateRangeAsync(utcWeekStart, endOfWeek);
                var ticketCount = await GetTicketCountForDateRangeAsync(utcWeekStart, endOfWeek);

                var result = new WeeklyRevenue
                {
                    WeekStart = utcWeekStart,
                    WeekEnd = endOfWeek,
                    Revenue = weeklyRevenue,
                    TicketCount = ticketCount
                };

                return new ApiResp().SetOk(result);
            }
            catch (Exception ex)
            {
                return new ApiResp().SetBadRequest(null, $"Error getting weekly revenue: {ex.Message}");
            }
        }

        public async Task<ApiResp> GetMonthlyRevenueAsync(int year, int month)
        {
            try
            {
                var startOfMonth = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
                var endOfMonth = startOfMonth.AddMonths(1).AddSeconds(-1);

                var monthlyRevenue = await GetRevenueForDateRangeAsync(startOfMonth, endOfMonth);
                var ticketCount = await GetTicketCountForDateRangeAsync(startOfMonth, endOfMonth);

                var result = new MonthlyRevenue
                {
                    Year = year,
                    Month = month,
                    Revenue = monthlyRevenue,
                    TicketCount = ticketCount
                };

                return new ApiResp().SetOk(result);
            }
            catch (Exception ex)
            {
                return new ApiResp().SetBadRequest(null, $"Error getting monthly revenue: {ex.Message}");
            }
        }

        private async Task<RevenueAnalytics> GetRevenueAnalyticsDataAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var now = DateTime.UtcNow;
            var today = now.Date;
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
            var startOfMonth = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var startOfYear = new DateTime(today.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var analytics = new RevenueAnalytics
            {
                TodayRevenue = await GetRevenueForDateRangeAsync(today, today.AddDays(1).AddSeconds(-1)),
                WeekRevenue = await GetRevenueForDateRangeAsync(startOfWeek, now),
                MonthRevenue = await GetRevenueForDateRangeAsync(startOfMonth, now),
                YearRevenue = await GetRevenueForDateRangeAsync(startOfYear, now)
            };

            // Get daily revenues for the last 30 days
            var thirtyDaysAgo = today.AddDays(-30);
            analytics.DailyRevenues = await GetDailyRevenuesAsync(thirtyDaysAgo, today);

            // Get weekly revenues for the last 12 weeks
            var twelveWeeksAgo = startOfWeek.AddDays(-84);
            analytics.WeeklyRevenues = await GetWeeklyRevenuesAsync(twelveWeeksAgo, startOfWeek);

            // Get monthly revenues for the last 12 months
            var twelveMonthsAgo = startOfMonth.AddMonths(-12);
            analytics.MonthlyRevenues = await GetMonthlyRevenuesAsync(twelveMonthsAgo, startOfMonth);

            return analytics;
        }

        public async Task<List<MovieRanking>> GetMovieRankingsDataAsync(int? limit = null)
        {
            // Get all successful payments
            var payments = await _uow.PaymentRepo.GetAllAsync(
                p => p.Status == PaymentStatus.Success && !p.IsDeleted,
                include: q => q.Include(p => p.Order)
            );

            // Get all seat schedules with showtimes and movies
            var seatSchedules = await _uow.SeatScheduleRepo.GetAllAsync(
                ss => payments.Select(p => p.OrderId).Contains(ss.OrderId.Value),
                include: q => q.Include(ss => ss.Showtime).ThenInclude(s => s.Movie)
            );

            // Group by movie and calculate total revenue from payments
            var movieRankings = seatSchedules
                .Where(ss => ss.Showtime?.Movie != null && !ss.Showtime.Movie.IsDeleted)
                .GroupBy(ss => new { ss.Showtime.Movie.Id, ss.Showtime.Movie.Title, ss.Showtime.Movie.Img })
                .Select(g => new MovieRanking
                {
                    MovieId = g.Key.Id,
                    MovieName = g.Key.Title ?? "Unknown Movie",
                    Poster = g.Key.Img ?? "",
                    TotalRevenue = g.Sum(ss => 
                        payments.FirstOrDefault(p => p.OrderId == ss.OrderId)?.AmountPaid ?? 0),
                    TotalTicketsSold = g.Count(),
                    AverageTicketPrice = g.Average(ss => 
                        payments.FirstOrDefault(p => p.OrderId == ss.OrderId)?.AmountPaid ?? 0)
                })
                .Where(mr => mr.TotalRevenue > 0)
                .OrderByDescending(x => x.TotalRevenue)
                .Take(limit ?? 10)
                .ToList();

            // Assign ranks based on revenue (highest revenue = rank 1)
            for (int i = 0; i < movieRankings.Count; i++)
            {
                movieRankings[i].Rank = i + 1;
            }

            return movieRankings;
        }

        public async Task<decimal> GetRevenueForDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            // Ensure UTC DateTime
            var utcStartDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            var utcEndDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);
            
            var payments = await _uow.PaymentRepo.GetAllAsync(
                p => p.Status == PaymentStatus.Success
                     && p.PaymentTime >= utcStartDate
                     && p.PaymentTime <= utcEndDate
                     && !p.IsDeleted
            );

            return payments.Sum(p => p.AmountPaid ?? 0);
        }

        public async Task<int> GetTicketCountForDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            // Ensure UTC DateTime
            var utcStartDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            var utcEndDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);
            
            var orderEntities = await _uow.OrderRepo.GetAllAsync(
                o => o.Status == OrderEnum.Success
                     && o.OrderTime >= utcStartDate
                     && o.OrderTime <= utcEndDate
                     && !o.IsDeleted,
                include: q => q.Include(o => o.SeatSchedules)
            );

            return orderEntities.Sum(o => o.SeatSchedules?.Count ?? 0);
        }

        private async Task<List<DailyRevenue>> GetDailyRevenuesAsync(DateTime startDate, DateTime endDate)
        {
            var dailyRevenues = new List<DailyRevenue>();
            var currentDate = startDate;

            while (currentDate <= endDate)
            {
                var nextDate = currentDate.AddDays(1);
                var revenue = await GetRevenueForDateRangeAsync(currentDate, nextDate.AddSeconds(-1));
                var ticketCount = await GetTicketCountForDateRangeAsync(currentDate, nextDate.AddSeconds(-1));

                dailyRevenues.Add(new DailyRevenue
                {
                    Date = currentDate,
                    Revenue = revenue,
                    TicketCount = ticketCount
                });

                currentDate = nextDate;
            }

            return dailyRevenues;
        }

        private async Task<List<WeeklyRevenue>> GetWeeklyRevenuesAsync(DateTime startDate, DateTime endDate)
        {
            var weeklyRevenues = new List<WeeklyRevenue>();
            var currentWeekStart = startDate;

            while (currentWeekStart <= endDate)
            {
                var weekEnd = currentWeekStart.AddDays(7).AddSeconds(-1);
                var revenue = await GetRevenueForDateRangeAsync(currentWeekStart, weekEnd);
                var ticketCount = await GetTicketCountForDateRangeAsync(currentWeekStart, weekEnd);

                weeklyRevenues.Add(new WeeklyRevenue
                {
                    WeekStart = currentWeekStart,
                    WeekEnd = weekEnd,
                    Revenue = revenue,
                    TicketCount = ticketCount
                });

                currentWeekStart = currentWeekStart.AddDays(7);
            }

            return weeklyRevenues;
        }

        private async Task<List<MonthlyRevenue>> GetMonthlyRevenuesAsync(DateTime startDate, DateTime endDate)
        {
            var monthlyRevenues = new List<MonthlyRevenue>();
            var currentMonthStart = startDate;

            while (currentMonthStart <= endDate)
            {
                var monthEnd = currentMonthStart.AddMonths(1).AddSeconds(-1);
                var revenue = await GetRevenueForDateRangeAsync(currentMonthStart, monthEnd);
                var ticketCount = await GetTicketCountForDateRangeAsync(currentMonthStart, monthEnd);

                monthlyRevenues.Add(new MonthlyRevenue
                {
                    Year = currentMonthStart.Year,
                    Month = currentMonthStart.Month,
                    Revenue = revenue,
                    TicketCount = ticketCount
                });

                currentMonthStart = currentMonthStart.AddMonths(1);
            }

            return monthlyRevenues;
        }
    }
} 