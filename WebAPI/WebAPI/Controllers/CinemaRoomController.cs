using Application.IRepos;
using Application.IServices;
using Application.ViewModel.Request;
using Application.ViewModel.Response;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
                return CreatedAtAction(nameof(CreateRoom), new { id = room.Id }, room);
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
                if (room == null)
                {
                    return NotFound(new { error = "Room not found" });
                }
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
                var updatedRoom = await _cinemaRoomService.UpdateRoomAsync(roomId, request);
                return Ok(updatedRoom);
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
            {
                return NotFound(new
                {
                    success = false,
                    errors = result.Errors
                });
            }

            return Ok(new
            {
                success = true,
                messages = result.Messages
            });
        }

    }
}
