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
        [HttpPost("CreateRedeem/{userId}")]
        public async Task<IActionResult> CreateRedeemAsync(Guid userId, [FromBody] List<RedeemRequest> requests)
        {
            var result = await _redeemService.CreateRedeemAsync(userId, requests);
            if (result.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound();
            }
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpGet("GetRedeemById/{id}")]
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
        [HttpGet("GetPandingRedeemsByAccount/{accountId}")]
        public async Task<IActionResult> GetPandingRedeemsByAccountAsync(Guid accountId)
        {
            var result = await _redeemService.GetPendingRedeemsByAccountAsync(accountId);
            if (result.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound();
            }
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpPut("CancelRedeem/{id}")]
        public async Task<IActionResult> CancelRedeemAsync(Guid id)
        {
            var result = await _redeemService.CancelRedeemAsync(id);
            if (result.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound();
            }
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpGet("GetPaidRedeemsByAccount/{accountId}")]
        public async Task<IActionResult> GetPaidRedeemsByAccountAsync(Guid accountId)
        {
            var result = await _redeemService.GetPaidRedeemsByAccountAsync(accountId);
            if (result.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound();
            }
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpPut("UpdateRedeem/{redeemId}")]
        public async Task<IActionResult> UpdateRedeemAsync(Guid redeemId, [FromBody] List<RedeemRequest> requests)
        {
            var result = await _redeemService.updateRedeemAsync(redeemId, requests);
            if (result.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound();
            }
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpPut("ReedemItem/{reedemId}")]
        public async Task<IActionResult> RedeemItem(Guid reedemId)
        {
            var result = await _redeemService.redeemItem(reedemId);
            if (result.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound();
            }
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}
