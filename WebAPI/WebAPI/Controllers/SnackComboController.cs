using Application.IRepos;
using Application.IServices;
using Application.Services;
using Application.ViewModel.Response;
using Application.ViewModel;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Application.ViewModel.Request;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SnackComboController : ControllerBase
    {
        private readonly ISnackComboService _snackComboService;
        private readonly ISnackService _snackService;

        public SnackComboController(ISnackComboService snackComboService, ISnackService snackService)
        {
            _snackComboService = snackComboService ?? throw new ArgumentNullException(nameof(snackComboService));
            _snackService = snackService ?? throw new ArgumentNullException(nameof(snackService));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddAsync([FromBody] SnackComboRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(new ApiResp().SetBadRequest(message: "Invalid model state."));
            var response = await _snackComboService.AddAsync(request);
            if (response.IsSuccess && response.Result is SnackComboResponse createdCombo)
            {
                return CreatedAtAction(nameof(GetComboWithItemsAsync), new { id = createdCombo.Id }, response);
            }
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateAsync([FromRoute] Guid id, [FromBody] SnackComboRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(new ApiResp().SetBadRequest(message: "Invalid model state."));
            var response = await _snackComboService.UpdateAsync(id, request);
            return StatusCode((int)response.StatusCode, response);
        }




        [HttpDelete("{id}")]    
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            try
            {
                await _snackComboService.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("with-snacks")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCombosWithSnacksAsync()
        {
            var response = await _snackComboService.GetCombosWithSnacksAsync();
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet("with-items/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetComboWithItemsAsync([FromRoute] Guid id)
        {
            var response = await _snackComboService.GetComboWithItemsAsync(id);
            return StatusCode((int)response.StatusCode, response);
        }
    }
}
