using Application.IServices;
using Application.ViewModel.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubscriptionController : ControllerBase
    {
        private readonly ISubscriptionService _subscriptionService;
        public SubscriptionController(ISubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }
        [HttpPost("CreateMembership")]
        public async Task<IActionResult> CreateSubscription([FromBody] SubscriptionRequest subscriptionRequest)
        {
            if (subscriptionRequest == null)
            {
                return BadRequest("Subscription request cannot be null.");
            }
            var response = await _subscriptionService.CreateSubscription(subscriptionRequest);
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return Unauthorized("User not authenticated!");
            }
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }
        [HttpGet("GetMembershipById/{id}")]
        public async Task<IActionResult> GetSubscriptionById(Guid id)
        {
            var response = await _subscriptionService.GetSubscriptionById(id);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound("Subscription does not exist!");
            }
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }
        [HttpGet("GetAllMemberships")]
        public async Task<IActionResult> GetAllSubscriptions()
        {
            var response = await _subscriptionService.GetAllSubscriptions();
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound("No subscriptions found!");
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return Unauthorized("User not authenticated!");
            }
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }
        [HttpPut("UpdateMembership/{id}")]
        public async Task<IActionResult> UpdateSubscription(Guid id, [FromBody] SubscriptionRequest subscriptionRequest)
        {
            if (subscriptionRequest == null)
            {
                return BadRequest("Subscription request cannot be null.");
            }
            var response = await _subscriptionService.UpdateSubscription(id, subscriptionRequest);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound("Subscription does not exist!");
            }
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }
        [HttpDelete("DeleteMembership/{id}")]
        public async Task<IActionResult> DeleteSubscription(Guid id)
        {
            var response = await _subscriptionService.DeleteSubscription(id);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound("Subscription does not exist!");
            }
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }
        [HttpPost("CancelMembership/{id}")]
        public async Task<IActionResult> CancelSubscription(Guid id)
        {
            var response = await _subscriptionService.CancelSubscription(id);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound("Subscription does not exist!");
            }
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }
    }
}
