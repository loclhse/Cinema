using Application.ViewModel;
using Application.ViewModel.Response;
using System;
using System.Threading.Tasks;

namespace Application.IServices
{
    public interface IDashboardService
    {
        Task<ApiResp> GetDashboardDataAsync();
        Task<ApiResp> GetRevenueAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<ApiResp> GetMovieRankingsAsync(int? limit = null);
        Task<ApiResp> GetDailyRevenueAsync(DateTime date);
        Task<ApiResp> GetWeeklyRevenueAsync(DateTime weekStart);
        Task<ApiResp> GetMonthlyRevenueAsync(int year, int month);
    }
} 