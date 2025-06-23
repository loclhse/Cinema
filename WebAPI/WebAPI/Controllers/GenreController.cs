using Application.IServices;
using Application.ViewModel.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

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
            if (result.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound();
            }
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpPut("UpdateGenre/{id}")]
        public async Task<IActionResult> UpdateGenre(Guid id, GenreRequest genreRequest)
        {
            var result = await _genreService.UpdateGenreAsync(id, genreRequest);
            if (result.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound();
            }
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpDelete("DeleteGenre/{id}")]
        public async Task<IActionResult> DeleteGenre(Guid id)
        {
            var result = await _genreService.DeleteGenreAsync(id);
            if (result.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound();
            }
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpGet("getGenreById/{id}")]
        public async Task<IActionResult> GetGenreById(Guid id)
        {
            var result = await _genreService.GetGenresAsync(id);
            if (result.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound();
            }
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}
