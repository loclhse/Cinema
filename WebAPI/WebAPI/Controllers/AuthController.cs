using Application.IServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Application.ViewModel.Request;
using Application.ViewModel.Response;
using Domain.Enums;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// POST: api/Auth/register
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] RegisterRequest req)
        {
            try
            {
                RegisterResponse registerResponse = await _authService.RegisterAsync(req);
                return CreatedAtAction(nameof(Register), registerResponse);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// POST: api/Auth/login
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            try
            {
                LoginResponse loginResponse = await _authService.LoginAsync(req);

                var refreshTokenCookieOptions = new CookieOptions
                {
                    HttpOnly = true, // Rất quan trọng: Cookie này không thể truy cập bằng JavaScript
                    Secure = true,   // Đặt là 'true' nếu bạn dùng HTTPS (khuyến nghị cho môi trường production)
                                     // Nếu bạn đang test trên localhost với HTTP, có thể tạm thời đặt 'false'.
                                     // NHƯNG PHẢI ĐẶT LẠI LÀ 'TRUE' KHI DEPLOY LÊN PRODUCTION!
                    Expires = loginResponse.RefreshTokenExpiryTime, // Sử dụng trực tiếp DateTime cho thuộc tính Expires
                    Path = "/",      // Cookie này có thể truy cập từ mọi đường dẫn trong domain
                    SameSite = SameSiteMode.Lax // Cân bằng giữa bảo mật và khả năng sử dụng.
                                                // 'Strict' an toàn hơn nhưng có thể hạn chế, 'None' dùng cho cross-site (yêu cầu Secure=true).
                };

                Response.Cookies.Append("refreshToken", loginResponse.RefreshToken, refreshTokenCookieOptions);

                return Ok(loginResponse);
            }
            catch (System.Exception ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
        }

        /// <summary>
        /// POST: api/Auth/refresh
        /// </summary>
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("refreshtoken")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshRequest model, CancellationToken ct = default)
        {
            try
            {
                AuthResponse authResponse = await _authService.RefreshTokenAsync(model.RefreshToken);
                return Ok(authResponse);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("changepassword/{id}")]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest pass, Guid id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _authService.ChangePasswordAsync(pass, id);
                if (result.Succeeded)
                {
                    return Ok(new { message = result });
                }
                else
                {
                    return BadRequest(new { error = result });
                }
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception)
            {
                // Log the exception here
                return StatusCode(500, new { error = "An error occurred while changing password." });
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            // Lấy access token từ header
            var accessToken = HttpContext.Request.Headers["Authorization"]
                .ToString().Replace("Bearer ", "");

            if (await _authService.IsAccessTokenBlacklistedAsync(accessToken))
            {
                // Nếu token đã bị blacklist, tức là nó đã bị thu hồi
                return Unauthorized(new { error = "Access token has been revoked." });
            }

            // Lấy refresh token từ cookie hoặc body
            string? refreshTokenFromCookie = HttpContext.Request.Cookies["refreshToken"];
            string? refreshTokenFromForm = null;

            // Chỉ cố gắng đọc từ Form nếu request có Content-Type là form
            if (HttpContext.Request.HasFormContentType)
            {
                try
                {
                    refreshTokenFromForm = Request.Form["refreshToken"];
                }
                catch (InvalidOperationException ex)
                {
                    // Ghi log lỗi nếu cần, nhưng không làm dừng tiến trình
                    _logger.LogWarning(ex, "Could not read form content, possibly due to missing Content-Type or malformed form data.");
                    // refreshTokenFromForm sẽ vẫn là null
                }
            }

            var refreshToken = refreshTokenFromCookie ?? refreshTokenFromForm;

            var result = await _authService.LogoutAsync(accessToken, refreshToken);

            if (!result.Succeeded)
            {
                return BadRequest(new { errors = result.Errors });
            }

            // Xóa cookie refresh token nếu có
            // Nhớ thêm CookieOptions nếu cần để xóa cookie hiệu quả
            var cookieOptions = new CookieOptions
            {
                Path = "/", // Hoặc đường dẫn cụ thể
                HttpOnly = true,
                Secure = true // true nếu dùng HTTPS
                              // SameSite = SameSiteMode.Lax, // Hoặc Strict/None tùy cấu hình
            };
            Response.Cookies.Delete("refreshToken", cookieOptions);

            return Ok(new { message = "Logout successful" });
        }

        [HttpPost("reset-password/{email}")]
        public async Task<IActionResult> ResetPassword(string email, [FromBody] ResetPasswordRequest model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _authService.ResetPasswordAsync(email, model);
                return Ok(new { message = result });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception)
            {
                // Log the exception here
                return StatusCode(500, new { error = "An error occurred while resetting password." });
            }
        }


    }
}
