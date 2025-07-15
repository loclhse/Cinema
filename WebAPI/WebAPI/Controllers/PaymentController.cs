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
                return Ok(new { PaymentUrl = url });
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
               
                return Ok(new { PaymentUrl = url });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[VNPay][Exception] {ex.Message}");
                return BadRequest(new { Message = $"Error creating VNPay payment URL: {ex.Message}" });
            }
        }

        [HttpGet("vnpay-return/order")]
        public async Task<IActionResult> PaymentReturnOrder()
        {
            var response = Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString());
            Console.WriteLine("VNPay Return Data: " + string.Join(", ", response.Select(kvp => $"{kvp.Key}={kvp.Value}")));
            var result = await _paymentService.HandleVnPayReturn(Request.Query);
            Console.WriteLine($"VNPay Result: {result}");
           
            if (!result.IsSuccess)
            {
                return Content($"Payment failed: {result.ErrorMessage ?? "Unknown error"}", "text/html");
            }
            return Content("Payment successful!", "text/html");
        }

        [HttpGet("vnpay-return/subscription")]
        public async Task<IActionResult> PaymentReturnSubscription()
        {
            var response = Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString());
            Console.WriteLine("VNPay Return Data: " + string.Join(", ", response.Select(kvp => $"{kvp.Key}={kvp.Value}")));
            var result = await _paymentService.HandleVnPayReturnForSubscription(Request.Query);
            Console.WriteLine($"VNPay Result: {result}");

            if (!result.IsSuccess)
            {
                return Content($"Payment failed: {result.ErrorMessage ?? "Unknown error"}", "text/html");
            }
            return Content("Payment successful!", "text/html");
        }
    }
} 