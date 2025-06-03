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

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// POST: api/Auth/register
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest req, CancellationToken ct = default)
        {
            try
            {
                RegisterResponse registerResponse = await _authService.RegisterAsync(req, ct);
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
        public async Task<IActionResult> Login([FromBody] LoginRequest req, CancellationToken ct = default)
        {
            try
            {
                LoginResponse loginResponse = await _authService.LoginAsync(req, ct);
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
        [Authorize(Roles = "Admin")]
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequestModel model, CancellationToken ct = default)
        {
            try
            {
                AuthResponse authResponse = await _authService.RefreshTokenAsync(model.RefreshToken, ct);
                return Ok(authResponse);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }

    public class RefreshRequestModel
    {
        public string RefreshToken { get; set; } = string.Empty;
    }
}
