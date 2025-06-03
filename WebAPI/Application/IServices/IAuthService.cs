using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        Task<RegisterResponse> RegisterAsync(RegisterRequest req, CancellationToken ct = default);
        Task<LoginResponse> LoginAsync(LoginRequest req, CancellationToken ct = default);
        Task<AuthResponse> RefreshTokenAsync(string refreshToken, CancellationToken ct = default);
        
    }
}
