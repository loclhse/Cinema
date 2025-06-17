using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;
using Application.Common;
using Application.IServices;
using Application.ViewModel.Request;
using Application.ViewModel.Response;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CinemaRoomController : ControllerBase
    {
        private readonly ICinemaRoomService _cinemaRoomService;

        public CinemaRoomController(ICinemaRoomService cinemaRoomService)
        {
            _cinemaRoomService = cinemaRoomService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateRoom([FromBody] CinemaRoomCreateRequest request)
        {
            try
            {
                var room = await _cinemaRoomService.CreateRoomAsync(request);
                return CreatedAtAction(nameof(GetRoomById), new { roomId = room.Id }, room);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("{roomId}")]
        public async Task<IActionResult> GetRoomById(Guid roomId)
        {
            try
            {
                var room = await _cinemaRoomService.GetRoomByIdAsync(roomId);
                if (room == null) return NotFound(new { error = "Room not found." });
                return Ok(room);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllRooms(int page = 1, int size = 10)
        {
            try
            {
                var rooms = await _cinemaRoomService.GetAllRoomsAsync(page, size);
                return Ok(rooms);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("{roomId}")]
        public async Task<IActionResult> UpdateRoom(Guid roomId, [FromBody] CinemaRoomUpdateRequest request)
        {
            try
            {
                var updated = await _cinemaRoomService.UpdateRoomAsync(roomId, request);
                return Ok(updated);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = "Room not found." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("{roomId}")]
        public async Task<IActionResult> DeleteRoom(Guid roomId)
        {
            var result = await _cinemaRoomService.DeleteRoomAsync(roomId);
            if (!result.Succeeded)
                return NotFound(new { success = false, errors = result.Errors });
            return Ok(new { success = true, messages = result.Messages });
        }


        [HttpPost("{roomId}/restore")]
        public async Task<IActionResult> RestoreRoom(Guid roomId)
        {
            var result = await _cinemaRoomService.RestoreRoomAsync(roomId);
            if (!result.Succeeded)
                return NotFound(new { success = false, errors = result.Errors });
            return Ok(new { success = true, messages = result.Messages });
        }

        [HttpGet("{roomId}/with-seats")]
        public async Task<IActionResult> GetRoomWithSeats(Guid roomId)
        {
            try
            {
                var dto = await _cinemaRoomService.GetRoomWithSeatsAsync(roomId);
                return Ok(dto);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = "Room not found." });
            }
        }

        [HttpGet("{roomId}/seat-matrix")]
        public async Task<IActionResult> GetSeatMatrix(Guid roomId)
        {
            try
            {
                var dto = await _cinemaRoomService.GetSeatMatrixAsync(roomId);
                return Ok(dto);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = "Room not found." });
            }
        }

        [HttpPost("{roomId}/generate-seats")]
        public async Task<IActionResult> GenerateSeats(Guid roomId, [FromBody] JsonDocument layoutJson)
        {
            var result = await _cinemaRoomService.GenerateSeatsFromLayoutAsync(roomId, layoutJson);

            if (!result.Succeeded)
            {
                // Trả về 404 nếu lỗi liên quan đến phòng không tồn tại
                if (result.Errors.Any(e => e.Contains("not found", StringComparison.OrdinalIgnoreCase)))
                    return NotFound(new { success = false, errors = result.Errors });

                return BadRequest(new { success = false, errors = result.Errors });
            }

            return Ok(new { success = true, messages = result.Messages });
        }
    }
}
