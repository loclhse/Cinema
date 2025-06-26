using Application.IServices;
using Application.ViewModel.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValidController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly IAuthService _authService; // cần inject thêm

        public ValidController(IEmailService emailService, IAuthService authService)
        {
            _emailService = emailService;
            _authService = authService;
        }

        [HttpPost("send-otp/{email}")]
        public async Task<IActionResult> SendOtp(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                    return BadRequest("Email is required.");

                await _emailService.SendPINForResetPassword(email);
                return Ok(new { message = "OTP sent to email." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Verify mã OTP hợp lệ cho email.
        /// Body JSON: { "email": "...", "pin": "123456" }
        /// </summary>
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                bool isValid = await _authService.VerifyOtpAsync(request.Email, request.Pin);
                if (!isValid)
                    return BadRequest(new { error = "OTP is invalid or expired." });

                return Ok(new { message = "OTP is valid." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "An error occurred while verifying OTP." });
            }
        }
    }
}
