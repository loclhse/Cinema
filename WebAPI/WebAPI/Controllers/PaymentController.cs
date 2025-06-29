using Application.IServices;
using Application.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
        }

       
        [HttpGet("user/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> FindPaymentByUserIdAsync(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                return BadRequest(new ApiResp().SetBadRequest("Invalid user ID format."));
            }

            var response = await _paymentService.FindPaymentByUserIdAsync(userId);
            return StatusCode((int)response.StatusCode, response);
        }

       
        [HttpGet("cash")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAllCashPaymentAsync()
        {
            var response = await _paymentService.GetAllCashPaymentAsync();
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPut("{id}/status/success")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ChangeStatusFromPendingToSuccessAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new ApiResp().SetBadRequest("Invalid payment ID format."));
            }

            var response = await _paymentService.ChangeStatusFromPendingToSuccessAsync(id);
            return StatusCode((int)response.StatusCode, response);
        }
    }
} 