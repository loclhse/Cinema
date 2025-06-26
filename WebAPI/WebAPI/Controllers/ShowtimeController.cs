using Application.IServices;
using Application.ViewModel.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShowtimeController : ControllerBase
    {
        private readonly IShowtimeService _showtimeService;
        public ShowtimeController(IShowtimeService showtimeService)
        {
            _showtimeService = showtimeService;
        }
        [HttpPost("CreateShowtime")]
        public async Task<IActionResult> CreateShowtime(ShowtimeResquest showtimeRequest, Guid movieId, Guid roomId)
        {
            var response = await _showtimeService.CreateShowtimeAsync(showtimeRequest, movieId,roomId);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }
        [HttpGet("GetAllShowtimes")]
        public async Task<IActionResult> GetAllShowtimes()
        {
            var response = await _showtimeService.GetAllShowtimesAsync();
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound();
            }
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }
        [HttpGet("GetShowtimeById/{id}")]
        public async Task<IActionResult> GetShowtimeById(Guid id)
        {
            var response = await _showtimeService.GetShowtimeByIdAsync(id);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound();
            }
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }
        [HttpPut("UpdateShowtime/{id}")]
        public async Task<IActionResult> UpdateShowtime(Guid id,  ShowtimeUpdateRequest showtimeUpdateRequest)
        {
            var response = await _showtimeService.UpdateShowtimeAsync(id, showtimeUpdateRequest);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound();
            }
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }
        [HttpDelete("DeleteShowtime/{id}")]
        public async Task<IActionResult> DeleteShowtime(Guid id)
        {
            var response = await _showtimeService.DeleteShowtimeAsync(id);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound();
            }
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }
        [HttpGet("GetShowtimeByMovieID/{id}")]
        public async Task<IActionResult> GetShowtimeByMovieID(Guid id)
        {
            var response = await _showtimeService.GetShowtimeByMovieIdAsync(id);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound();
            }
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }
    }
}
