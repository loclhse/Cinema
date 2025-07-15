using Application.IServices;
using Application.ViewModel.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RedeemController : ControllerBase
    {
        private readonly IRedeemService _redeemService;
        public RedeemController(IRedeemService redeemService)
        {
            _redeemService = redeemService;
        }
        [HttpPost("CreateRedeem")]
        public async Task<IActionResult> CreateRedeemAsync([FromBody] RedeemRequest request)
        {
            var result = await _redeemService.CreateRedeemAsync(request);
            if (result.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound();
            }
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpGet("GetRedeemById{id}")]
        public async Task<IActionResult> GetRedeemByIdAsync(Guid id)
        {
            var result = await _redeemService.GetRedeemAsync(id);
            if (result.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound();
            }
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpGet("GetAllRedeems")]
        public async Task<IActionResult> GetAllRedeemsAsync()
        {
            var result = await _redeemService.GetAllRedeemsAsync();
            if (result.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound();
            }
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpGet("GetRedeemsByAccount{accountId}")]
        public async Task<IActionResult> GetRedeemsByAccountAsync(Guid accountId)
        {
            var result = await _redeemService.GetRedeemsByAccountAsync(accountId);
            if (result.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound();
            }
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpPut("CancelRedeem{id}")]
        public async Task<IActionResult> CancelRedeemAsync(Guid id)
        {
            var result = await _redeemService.CancelRedeemAsync(id);
            if (result.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound();
            }
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    } 
}
