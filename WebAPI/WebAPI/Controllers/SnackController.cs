using Application.IServices;
using Application.Services;
using Application.ViewModel;
using Application.ViewModel.Request;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SnackController : ControllerBase
    {
        private readonly ISnackService _snackService;

        public SnackController(ISnackService snackService)
        {
            _snackService = snackService ?? throw new ArgumentNullException(nameof(snackService));
        }

        [HttpGet("get-snack-by-id/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByIdAsync([FromRoute] Guid id)
        {
            var response = await _snackService.GetByIdAsync(id);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet("get-all-snack")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllAsync()
        {
            var response = await _snackService.GetAllAsync();
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPost("create-snack")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddAsync([FromBody] SnackRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(new ApiResp().SetBadRequest(message: "Invalid model state."));
            var response = await _snackService.AddAsync(request);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPut("update-snack/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateAsync([FromRoute] Guid id, [FromBody] SnackRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(new ApiResp().SetBadRequest(message: "Invalid model state."));
            var response = await _snackService.UpdateAsync(id, request);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpDelete("delete-snack/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAsync([FromRoute] Guid id)
        {
            var response = await _snackService.DeleteAsync(id);
            return StatusCode((int)response.StatusCode, response);
        }

       
    }
}
