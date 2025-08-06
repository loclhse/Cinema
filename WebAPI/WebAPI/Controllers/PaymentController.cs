using Application;
using Application.IServices;
using Application.ViewModel;
using Infrastructure.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using WebAPI.Infrastructure.Services;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IUnitOfWork _uow;
        private readonly IVnPayService _vnPayService;


       
        public PaymentController(IPaymentService paymentService,IUnitOfWork uow,IVnPayService vnPayService)
        {
            _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _vnPayService=vnPayService ?? throw new ArgumentNullException(nameof(vnPayService));

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

        [HttpPost("CreateVnPayPaymentUrlForOrder")]
        public async Task<IActionResult> CreateVnPayPaymentUrl([FromQuery] Guid orderId)
        {
            try
            {
                var order = await _uow.OrderRepo.GetByIdAsync(orderId);
                if (order == null)
                    return NotFound(new { Message = "Order not found." });

                var url = _vnPayService.CreatePaymentUrl(order, HttpContext);
                return Ok(url);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[VNPay][Exception] {ex.Message}");
                return BadRequest(new { Message = $"Error creating VNPay payment URL: {ex.Message}" });
            }
        }

        [HttpPost("CreateVnPayPaymentUrlForSubcription")]
        public async Task<IActionResult> CreateVnPayPaymentUrlForSubcription([FromQuery] Guid subscriptionId)
        {
            try
            {
                var subscription = await _uow.SubscriptionRepo.GetByIdAsync(subscriptionId);
                if (subscription == null)
                    return NotFound(new { Message = "Order not found." });
                
                var url = _vnPayService.CreatePaymentUrlForSubscription(subscription, HttpContext);
               
                return Ok(url);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[VNPay][Exception] {ex.Message}");
                return BadRequest(new { Message = $"Error creating VNPay payment URL: {ex.Message}" });
            }
        }

        

        /// <summary>
        /// Process VNPay callback for order payments
        /// </summary>
        /// <param name="callbackData">VNPay callback data containing payment information</param>
        /// <returns>Payment processing result</returns>
        [HttpPost("process-vnpay-callback")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ProcessVnPayCallback([FromBody] Dictionary<string, string> callbackData)
        {
            try
            {
                // Convert the dictionary back to IQueryCollection format
                var queryCollection = new QueryCollection(callbackData.ToDictionary(x => x.Key, x => new Microsoft.Extensions.Primitives.StringValues(x.Value)));
                
                var result = await _paymentService.HandleVnPayReturn(queryCollection);
                
                if (result.IsSuccess)
                {
                    return Ok(new { success = true, message = "Payment processed successfully" });
                }
                else
                {
                    return BadRequest(new { success = false, message = result.ErrorMessage });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = $"Error processing payment: {ex.Message}" });
            }
        }

        /// <summary>
        /// Process VNPay callback for subscription payments
        /// </summary>
        /// <param name="callbackData">VNPay callback data containing subscription payment information</param>
        /// <returns>Subscription payment processing result</returns>
        [HttpPost("process-vnpay-callback-subscription")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ProcessVnPayCallbackSubscription([FromBody] Dictionary<string, string> callbackData)
        {
            try
            {
                // Convert the dictionary back to IQueryCollection format
                var queryCollection = new QueryCollection(callbackData.ToDictionary(x => x.Key, x => new Microsoft.Extensions.Primitives.StringValues(x.Value)));
                
                var result = await _paymentService.HandleVnPayReturnForSubscription(queryCollection);
                
                if (result.IsSuccess)
                {
                    return Ok(new { success = true, message = "Subscription payment processed successfully" });
                }
                else
                {
                    return BadRequest(new { success = false, message = result.ErrorMessage });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = $"Error processing subscription payment: {ex.Message}" });
            }
        }
    }
} 