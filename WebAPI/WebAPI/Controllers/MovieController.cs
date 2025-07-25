﻿using Application.IServices;
using Application.ViewModel.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Net;

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

        [HttpGet("Search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SearchMovies([FromQuery] string? searchTerm, [FromQuery] string searchType, [FromQuery] int? limit = 5)
        {
#pragma warning disable CS8604 // Possible null reference argument.
            var response = await _movieService.SearchMoviesAsync(searchTerm, searchType, limit);
#pragma warning restore CS8604 // Possible null reference argument.
            if (response.IsSuccess)
            {
                return Ok(response);
            }
            else
            {
                return StatusCode((int)response.StatusCode, response);
            }
        }

        [HttpPost("CreateMovie")]
        public async Task<IActionResult> CreateMovie(MovieRequest movieRequest)
        {
            var response = await _movieService.CreateMovieAsync(movieRequest);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }
        [HttpGet("GetAllMovies")]
        public async Task<IActionResult> GetAllMovies()
        {
            var response = await _movieService.GetAllMoviesAsync();
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound();
            }
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }
        [HttpGet("GetMovieById{id}")]
        public async Task<IActionResult> GetMovieById(Guid id)
        {
            var response = await _movieService.GetMovieByIdAsync(id);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound();
            }
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }
        [HttpPut("UpdateMovie{id}")]
        public async Task<IActionResult> UpdateMovie(Guid id, [FromBody] MovieRequest movieRequest)
        {
            var response = await _movieService.UpdateMovieAsync(id, movieRequest);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound();
            }
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }
        [HttpDelete("DeleteMovie{id}")]
        public async Task<IActionResult> DeleteMovie(Guid id)
        {
            var response = await _movieService.DeleteMovieAsync(id);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound();
            }
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }
        [HttpGet("ElasticSearchMovie")]
        public async Task<IActionResult> ElasticSearchMoive(string keyword)
        {
            var response = await _movieService.ElasticSearchMovie(keyword);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound();
            }
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }




    }
}
