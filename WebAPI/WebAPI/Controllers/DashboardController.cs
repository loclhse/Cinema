using Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize(Roles = "Admin")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        /// <summary>
        /// Get complete dashboard data including revenue analytics and movie rankings
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetDashboardData()
        {
            var result = await _dashboardService.GetDashboardDataAsync();
            return Ok(result);
        }

        /// <summary>
        /// Get revenue analytics for a specific date range
        /// </summary>
        [HttpGet("revenue")]
        public async Task<IActionResult> GetRevenueAnalytics([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            var result = await _dashboardService.GetRevenueAnalyticsAsync(startDate, endDate);
            return Ok(result);
        }

        /// <summary>
        /// Get movie rankings based on ticket sales revenue
        /// </summary>
        [HttpGet("movie-rankings")]
        public async Task<IActionResult> GetMovieRankings([FromQuery] int? limit = null)
        {
            var result = await _dashboardService.GetMovieRankingsAsync(limit);
            return Ok(result);
        }

        /// <summary>
        /// Get daily revenue for a specific date
        /// </summary>
        [HttpGet("revenue/daily")]
        public async Task<IActionResult> GetDailyRevenue([FromQuery] DateTime date)
        {
            var result = await _dashboardService.GetDailyRevenueAsync(date);
            return Ok(result);
        }

        /// <summary>
        /// Get weekly revenue for a specific week
        /// </summary>
        [HttpGet("revenue/weekly")]
        public async Task<IActionResult> GetWeeklyRevenue([FromQuery] DateTime weekStart)
        {
            var result = await _dashboardService.GetWeeklyRevenueAsync(weekStart);
            return Ok(result);
        }

        /// <summary>
        /// Get monthly revenue for a specific month
        /// </summary>
        [HttpGet("revenue/monthly")]
        public async Task<IActionResult> GetMonthlyRevenue([FromQuery] int year, [FromQuery] int month)
        {
            var result = await _dashboardService.GetMonthlyRevenueAsync(year, month);
            return Ok(result);
        }
    }
} 