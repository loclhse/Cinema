using Application.IServices;
using Application.ViewModel.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
            if (seatSchedules == null || !seatSchedules.Any())
            {
                return NotFound($"No seat schedules found for showtime with ID: {id}");
            }

            return Ok(seatSchedules);
        }

        [HttpPut("HoldSeats")]
        public async Task<IActionResult> HoldSeats([FromBody] HoldSeatRequest request)
        {
            var heldSeats = await _seatScheduleService.HoldSeatAsync(request.SeatIds);
            return Ok(heldSeats);
        }

        [HttpPut("UpdateSeatStatus")]
        public async Task<IActionResult> UpdateSeatStatus([FromBody] UpdateSeatStatusRequest request)
        {
            var result = await _seatScheduleService.UpdateSeatStatusAsync(request.SeatScheduleIds, request.Status);
            if (result.Succeeded)
                return Ok(result);
            return BadRequest(result);
        }
    }
}
