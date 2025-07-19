using Application.IServices;
using Application.Services;
using Application.ViewModel.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Hubs;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SeatScheduleController : ControllerBase
    {
        private readonly ISeatScheduleService _seatScheduleService;
        public SeatScheduleController(ISeatScheduleService seatScheduleService)
        {
            _seatScheduleService = seatScheduleService;
        }

        [HttpGet("GetSeatSchedulesByShowtime/{id}")]
        public async Task<IActionResult> GetSeatSchedulesByShowtimeAsync(Guid id)
        {
            // Giả sử bạn có một service hoặc DbContext để lấy dữ liệu
            var seatSchedules = await _seatScheduleService.GetSeatSchedulesByShowtimeAsync(id);

            // Nếu không tìm thấy lịch ghế nào
            if (seatSchedules == null || !seatSchedules.Any()) // Fix: Ensure seatSchedules is a collection
            {
                return NotFound($"No seat schedules found for showtime with ID: {id}");
            }

            return Ok(seatSchedules);
        }

        [HttpGet("GetShowTimeBySeatSchedule/{id}")]
        public async Task<IActionResult> GetShowTimeBySeatScheduleAsync(Guid id)
        {
            // Giả sử bạn có một service hoặc DbContext để lấy dữ liệu
            var seatSchedules = await _seatScheduleService.GetShowTimeBySeatScheduleAsync(id);

            // Nếu không tìm thấy lịch ghế nào
            if (seatSchedules == null)
            {
                return NotFound($"No seat schedules found for showtime with ID: {id}");
            }

            return Ok(seatSchedules);
        }

        [HttpGet("GetHoldSeatByUserId/{showTimeId},{userId}")]
        public async Task<IActionResult> GetHoldSeatByUserIdAsync(Guid showTimeId, Guid userId)
        {
            // Giả sử bạn có một service hoặc DbContext để lấy dữ liệu
            var seatSchedules = await _seatScheduleService.GetHoldSeatByUserIdAsync(showTimeId, userId);

            // Nếu không tìm thấy lịch ghế nào
            if (seatSchedules == null || !seatSchedules.Any()) 
            {
                return NotFound($"No hold seat found for showtime with user ID: {userId}");
            }

            return Ok(seatSchedules);
        }

        [HttpPut("HoldSeats")]
        public async Task<IActionResult> HoldSeats([FromBody] HoldSeatRequest request)
        {
            var userId = User.GetUserId();
            if (userId == Guid.Empty) // Check for Guid.Empty instead of null
            {
                return BadRequest("Please login before buying.");
            }
            var connectionId = Request.Headers["Connection-Id"].ToString(); // gửi từ SignalR hoặc FE

            try
            {
                var result = await _seatScheduleService.HoldSeatAsync(request.ShowtimeId, request.SeatIds, userId, connectionId);
                return Ok(result);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("ConfirmSeats")]
        public async Task<IActionResult> ConfirmSeats([FromBody] List<Guid> seatIds)
        {
            var userId = User.GetUserId();
            if (userId == Guid.Empty) // Check for Guid.Empty instead of null
            {
                return BadRequest("Please login before buying.");
            }

            var result = await _seatScheduleService.ConfirmSeatAsync(seatIds, userId);

            return result?.Succeeded == true ? Ok(result) : BadRequest(result);
        }

        [HttpPut("CancelHold")]
        public async Task<IActionResult> CancelHold([FromBody] List<Guid> seatIds)
        {
            var userId = User.GetUserId();
            if (userId == Guid.Empty) // Check for Guid.Empty instead of null
            {
                return BadRequest("Please login before buying.");
            }

            var result = await _seatScheduleService.CancelHoldAsync(seatIds, userId);

            return result?.Succeeded == true ? Ok(result) : BadRequest(result);
        }

        [HttpPut("CancelHoldByConnection")]
        public async Task<IActionResult> CancelHoldByConnection([FromQuery] string connectionId)
        {
            var userId = User.GetUserId();
            if (userId == Guid.Empty) // Check for Guid.Empty instead of null
            {
                return BadRequest("Please login before buying.");
            }

            if (string.IsNullOrWhiteSpace(connectionId))
                return BadRequest("Missing connectionId.");

            await _seatScheduleService.CancelHoldByConnectionAsync(connectionId, userId);
            return Ok("Hold released for disconnected connection.");
        }

        [HttpPut("UpdateSeatStatus")]
        //[Authorize(Roles = "Admin")] // nếu bạn muốn chỉ admin được quyền
        public async Task<IActionResult> UpdateSeatStatus([FromBody] UpdateSeatStatusRequest request)
        {
            var result = await _seatScheduleService.UpdateSeatStatusAsync(
                request.SeatScheduleIds, request.Status);

            return result.Succeeded ? Ok(result) : BadRequest(result);
        }
    }
}
