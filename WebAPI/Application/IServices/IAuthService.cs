using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using Application.ViewModel.Request;
using Application.ViewModel.Response;

namespace Application.IServices
{
    public interface IAuthService
    {
        /// <summary>
        /// IAuthService định nghĩa các phương thức:
        /// - RegisterAsync: đăng ký user mới
        /// - LoginAsync: đăng nhập
        /// - RefreshTokenAsync: cấp lại access token từ refresh token
        /// </summary>
        Task<RegisterResponse> RegisterAsync(RegisterRequest req);
        Task<LoginResponse> LoginAsync(LoginRequest req);
        Task<AuthResponse> RefreshTokenAsync(string refreshToken);
        Task<OperationResult> ChangePasswordAsync(ChangePasswordRequest model, Guid id);
        Task <OperationResult> LogoutAsync(string accessToken, string? refreshToken);
        Task<bool> IsAccessTokenBlacklistedAsync(string accessToken);
    }
}
