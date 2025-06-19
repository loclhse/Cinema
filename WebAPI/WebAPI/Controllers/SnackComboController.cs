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

        [HttpPost("create-snackcombo")]
       
        
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

        [HttpPut("update-snackcombo/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateAsync([FromRoute] Guid id, [FromBody] SnackComboUpdateRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(new ApiResp().SetBadRequest(message: "Invalid model state."));
            var response = await _snackComboService.UpdateAsync(id, request);
            return StatusCode((int)response.StatusCode, response);
        }




        [HttpDelete("delete-snackcombo/{id}")]    
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

        [HttpGet("get-all-snackcombo")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCombosWithSnacksAsync()
        {
            var response = await _snackComboService.GetCombosWithSnacksAsync();
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet("get-snackcombo-by-id/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetComboWithItemsAsync([FromRoute] Guid id)
        {
            var response = await _snackComboService.GetComboWithItemsAsync(id);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpDelete("remove-snack-from-combo/{comboId}/{snackId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteSnackFromComboAsync([FromRoute] Guid comboId, [FromRoute] Guid snackId)
        {
            var response = await _snackComboService.DeleteSnackFromComboAsync(comboId, snackId);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPut("update-snack-quantity/{comboId}/{snackId}/{quantity}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateSnackQuantityInComboAsync([FromRoute] Guid comboId, [FromRoute] Guid snackId, [FromRoute] int quantity)
        {
            var response = await _snackComboService.UpdateSnackQuantityInComboAsync(comboId, snackId, quantity);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPost("add-snack-to-combo/{comboId}/{snackId}/{quantity}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddSnackToComboAsync([FromRoute] Guid comboId, [FromRoute] Guid snackId, [FromRoute] int quantity)
        {
            var response = await _snackComboService.AddSnackToComboAsync(comboId, snackId, quantity);
            return StatusCode((int)response.StatusCode, response);
        }
    }
}
