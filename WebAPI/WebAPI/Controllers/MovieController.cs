using Application.IServices;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MovieController : ControllerBase
    {
        private readonly IMovieService _movieService;

        public MovieController(IMovieService movieService)
        {
            _movieService = movieService ?? throw new ArgumentNullException(nameof(movieService));
        }
        [HttpGet("search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SearchMovies([FromQuery] string? searchTerm, [FromQuery] string searchType, [FromQuery] int? limit = 5)
        {
            var response = await _movieService.SearchMoviesAsync(searchTerm, searchType, limit);
            if (response.IsSuccess)
            {
                return Ok(response);
            }
            else
            {
                return StatusCode((int)response.StatusCode, response);
            }
        }
    }
}
