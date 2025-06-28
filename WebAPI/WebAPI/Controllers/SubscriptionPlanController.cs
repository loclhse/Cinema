using Application.IServices;
using Application.ViewModel.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubscriptionPlanController : ControllerBase
    {
        private readonly ISubscriptionPlanService _subscriptionPlanService;
        public SubscriptionPlanController(ISubscriptionPlanService subscriptionPlanService)
        {
            _subscriptionPlanService = subscriptionPlanService;
        }
        [HttpPost("create")]
        public async Task<IActionResult> CreateNewSubscriptionPlan(SubscriptionPlanRequest request)
        {
            var response = await _subscriptionPlanService.CreateNewSubscriptionPlanAsync(request);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }
        [HttpPost("activate/{id}")]
        public async Task<IActionResult> ActivateSubscriptionPlan(Guid id)
        {
            var response = await _subscriptionPlanService.ActiveSubscriptionPlanAsync(id);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound();
            }
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }
        [HttpGet("userGetAllPlan")]
        public async Task<IActionResult> UserGetAllSubscriptionPlans()
        {
            var response = await _subscriptionPlanService.UserGetAllSubscriptionPlansAsync();
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound();
            }
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }
        [HttpGet("getById/{id}")]
        public async Task<IActionResult> GetSubscriptionPlanById(Guid id)
        {
            var response = await _subscriptionPlanService.GetSubscriptionPlanByIdAsync(id);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound();
            }
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }
        [HttpGet("managerGetAllPlan")]
        public async Task<IActionResult> ManagerGetAllSubscriptionPlans()
        {
            var response = await _subscriptionPlanService.ManagerGetAllSubscriptionPlansAsync();
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound();
            }
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteSubscriptionPlan(Guid id)
        {
            var response = await _subscriptionPlanService.DeleteSubscriptionPlanAsync(id);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound();
            }
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateInActiveSubscriptionPlan(Guid id, SubscriptionPlanRequest request)
        {
            var response = await _subscriptionPlanService.UpdateInActiveSubscriptionPlanAsync(id, request);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound();
            }
            return response.IsSuccess ? Ok(response) : BadRequest(response);

        }
        [HttpGet("ViewHistory")]
        public async Task<IActionResult> ViewSubscriptionPlanHistory()
        {
            var response = await _subscriptionPlanService.ManagerGetAllSubscriptionPlansHistoryAsync();
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound();
            }
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }
    }
}
