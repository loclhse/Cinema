using Application.IServices;
using Application.ViewModel.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GenreController : ControllerBase
    {
        private readonly IGenreService _genreService;
        public GenreController(IGenreService genreService)
        {
            _genreService = genreService;
        }
        [HttpPost("CreateGenre")]
        public async Task<IActionResult> CreateGenre(GenreRequest genreRequest)
        {
            var result = await _genreService.CreateGenreAsync(genreRequest);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpGet("GetAllGenre")]
        public async Task<IActionResult> GetAllGenre()
        {
            var result = await _genreService.GetAllGenresAsync();
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }
        [HttpPut("UpdateGenre/{id}")]
        public async Task<IActionResult> UpdateGenre(Guid id, GenreRequest genreRequest)
        {
            var result = await _genreService.UpdateGenreAsync(id, genreRequest);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }
        [HttpDelete("DeleteGenre/{id}")]
        public async Task<IActionResult> DeleteGenre(Guid id)
        {
            var result = await _genreService.DeleteGenreAsync(id);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }
        [HttpGet("getGenreById/{id}")]
        public async Task<IActionResult> GetGenreById(Guid id)
        {
            var result = await _genreService.GetGenresAsync(id);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }
    }
}
