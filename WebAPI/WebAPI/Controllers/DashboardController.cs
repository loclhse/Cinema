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
        /// Get complete dashboard data including movie rankings
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetDashboardData()
        {
            var result = await _dashboardService.GetDashboardDataAsync();
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
    }
} 