using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.IServices;
using Application.ViewModel.Request;
using Application.ViewModel.Response;
using Application.Exceptions;
using Domain.Entities;
using Application.IRepos;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Application;
using Application.Domain;
using Domain.Enums;
using Infrastructure.MapperConfigs;
using Application.Common;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Repos;

namespace Infrastructure.Services
{
    /// <summary>
    /// AuthService triển khai IAuthService (ở tầng Infrastructure).
    /// - Register: tạo ApplicationUser + AppUser, gán role, sinh Access+Refresh token
    /// - Login: kiểm tra mật khẩu, sinh Access+Refresh token
    /// - RefreshToken: cấp lại Access+Refresh token khi có refresh token hợp lệ
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _uow;
        private readonly IJwtTokenGenerator _tokenGen;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public AuthService(
           IUnitOfWork uow,
           IJwtTokenGenerator tokenGen,
           IConfiguration configuration,
           ILogger<AuthService> logger,
           SignInManager<ApplicationUser> signInManager,
           UserManager<ApplicationUser> userManager)
        {
            _uow = uow;
            _tokenGen = tokenGen;
            _configuration = configuration;
            _logger = logger; // No change needed here after fixing the type
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public async Task<RegisterResponse> RegisterAsync(RegisterRequest req)
        {
            // 1. Sinh GUID cho user
            var newUserId = Guid.NewGuid();
            // 2. Map từ DTO → DomainUser (Identity) và AppUser (business)
            var domainUser = req.ToDomainUser(newUserId);
            var appUser = req.ToAppUser(newUserId);

            // 3. Bắt đầu transaction
            await using var tx = await _uow.BeginTransactionAsync();
            try
            {
                // 4. Tạo ApplicationUser (AspNetUsers) từ DomainUser
                var createResult = await _uow.AuthRepo.CreateUserAsync(domainUser, req.Password);
                if (!createResult.Succeeded)
                {
                    var errors = string.Join("; ", createResult.Errors);
                    throw new InvalidOperationException($"Unable to create account Identity: {errors}");
                }
                //Note: CreateUserAsync đã gọi SaveChanges, nhưng vì đang trong transaction nên sẽ rollback nếu có lỗi sau này.
                //Identity gọi SaveChanges() sớm (cộng thêm AcceptAllChanges())
                //An toàn rồi, nhưng nếu muốn “đẹp” hơn, bạn có thể:
                //• Viết wrapper CreateUserAsyncNoAccept() dùng SaveChanges(false) và tự AcceptAllChanges() sau khi toàn bộ nghiệp vụ thành công.
                //• Hoặc tách Identity(user + role) ra ngoài transaction, rồi mở transaction mới cho AppUser/ RefreshToken(đã bàn).

                // 5. Xác thực / mapping role
                var validRoles = new[]
                {
                    AppRoleNames.Admin,
                    AppRoleNames.Employee,
                    AppRoleNames.Member,
                    AppRoleNames.Customer
                };
                var matchedRole = validRoles.FirstOrDefault(r => string.Equals(r, req.Role?.Trim(), StringComparison.OrdinalIgnoreCase)) ??AppRoleNames.Customer;

                if (!validRoles.Contains(matchedRole))
                {
                    throw new InvalidOperationException(
                        $"Role '{req.Role}' Invalid. Only accepted: {string.Join(", ", validRoles)}");
                }

                // 6. Gán role cho user
                var addRoleResult = await _uow.AuthRepo.AddUserToRoleAsync(newUserId, matchedRole);
                if (!addRoleResult.Succeeded)
                {
                    var roleErrors = string.Join("; ", addRoleResult.Errors);
                    throw new InvalidOperationException($"Cannot assign role: {roleErrors}");
                }

                // 7. Thêm AppUser (business)
                await _uow.UserRepo.AddAsync(appUser);

                // 8. Lưu tất cả thay đổi: 
                // - Tất cả SaveChanges của CreateUserAsync và AddToRoleAsync đều nằm trong cùng transaction.
                await _uow.SaveChangesAsync();

                // 9. Commit transaction
                await tx.CommitAsync();

                // 10. Map DomainUser + AppUser + role → RegisterResponse (dùng mapper)
                return domainUser.ToRegisterResponse(appUser, matchedRole);
            }
            catch
            {
                // Nếu có bất kỳ lỗi nào (tạo Identity, gán role, thêm AppUser, thêm RefreshToken, SaveChanges…)
                // thì rollback toàn bộ. Nhờ đó, dù CreateUserAsync đã gọi SaveChanges, nó vẫn rollback vì
                // đang nằm trong transaction chung.
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest req)
        {
            // 1. Lấy DomainUser (Identity), AppUser (business) và roles
            // domainUser
            var (domainUser, appUser, roles) = await _uow.AuthRepo.GetUserWithRolesAndProfileAsync(req.UserName);

            if (domainUser == null || appUser == null)
                throw new InvalidOperationException("Account does not exist");

            // 2. Check mật khẩu
            var passwordValid = await _uow.AuthRepo.CheckPasswordAsync(req.UserName, req.Password);
            if (!passwordValid)
                throw new InvalidOperationException("Incorrect username or password");

            // 3. Sinh AccessToken (JWT) dựa trên chính DomainUser + roles
            var (accessToken, accessExpire) = await _tokenGen.GenerateAccessTokenAsync(domainUser, roles);

            // 4. Sinh RefreshToken mới (trả về chuỗi, expiration và entity)
            var (refreshTokenValue, refreshExpireAt, refreshTokenEntity) = await _tokenGen.GenerateRefreshTokenAsync(domainUser.Id);

            // 5. Lưu RefreshToken entity vào DB
            await _uow.AuthRepo.AddRefreshTokenAsync(refreshTokenEntity);
            await _uow.SaveChangesAsync();

            // 6. Lấy role chính (nếu cần trả cho client)
            var roleName = roles.FirstOrDefault() ?? AppRoleNames.Customer;

            // 7. Trả về LoginResponse (DomainUser đã có sẵn, AppUser, role, accessToken, refreshToken)
            return domainUser.ToLoginResponse(
                appUser,
                roleName,
                accessToken,
                refreshTokenValue,
                refreshExpireAt);
        }

        public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
        {
            // 1. Lấy RefreshToken entity cũ
            var existing = await _uow.AuthRepo.GetRefreshTokenAsync(refreshToken);
            if (existing == null || existing.Revoked || existing.ExpiresAt < DateTime.UtcNow)
                throw new InvalidOperationException("Refresh token is invalid or expired");

            // 2. Lấy DomainUser + roles bằng UserId
            var (domainUser, roles) = await _uow.AuthRepo.GetUserWithRolesAsyncById(existing.UserId);
            if (domainUser == null)
                throw new InvalidOperationException("User does not exist");

            // 3. Sinh AccessToken mới
            var (newAccessToken, newAccessExpire) = await _tokenGen.GenerateAccessTokenAsync(domainUser, roles);

            // 4. Thu hồi RefreshToken cũ
            await _uow.AuthRepo.RevokeRefreshTokenAsync(existing);

            // 5. Sinh RefreshToken mới
            var (newRefreshValue, newRefreshExpireAt, newRefreshEntity) = await _tokenGen.GenerateRefreshTokenAsync(domainUser.Id);

            // 6. Lưu entity mới vào DB
            await _uow.AuthRepo.AddRefreshTokenAsync(newRefreshEntity);
            await _uow.SaveChangesAsync();

            // 7. Trả về AuthResponse (bao gồm cả các expiration nếu bạn muốn)
            return new AuthResponse
            {
                AccessToken = newAccessToken,
                AccessTokenExpiration = newAccessExpire,
                RefreshToken = newRefreshValue,
                RefreshTokenExpiration = newRefreshExpireAt
            };
        }

        public async Task<OperationResult> ChangePasswordAsync(ChangePasswordRequest model, Guid userId)
        {
            try
            {
                // Tìm user bằng Id
                var user = await _uow.UserRepo.GetAsync(x => x.Id == userId);
                if (user == null)
                {
                    _logger.LogWarning("User not found with ID: {UserId}", userId);
                    return OperationResult.Failed(["User not found."]);
                }

                // Validate input passwords
                if (string.IsNullOrWhiteSpace(model.CurrentPassword))
                {
                    _logger.LogWarning("Current password is null or empty for user {UserId}", userId);
                    return OperationResult.Failed(["Current password cannot be null or empty."]);
                }

                if (string.IsNullOrWhiteSpace(model.NewPassword))
                {
                    _logger.LogWarning("New password is null or empty for user {UserId}", userId);
                    return OperationResult.Failed(["New password cannot be null or empty."]);
                }

                // Đổi mật khẩu
                var result = await _uow.AuthRepo.ChangePasswordAsync(userId, model.CurrentPassword, model.NewPassword);

                if (!result.Succeeded)
                {
                    var errors = result.Errors.ToArray();
                    _logger.LogWarning("Password change failed for user {UserId}: {Errors}", userId, string.Join(", ", errors));
                    return OperationResult.Failed(errors);
                }

                // Đăng xuất sau khi đổi mật khẩu
                await _signInManager.SignOutAsync();

                _logger.LogInformation("Password changed successfully for user {UserId}", userId);
                return OperationResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user {UserId}", userId);
                return OperationResult.Failed(["An unexpected error occurred while changing password."]);
            }
        }

        public async Task<OperationResult> LogoutAsync(string accessToken, string? refreshToken = null)
        {
            using var transaction = await _uow.BeginTransactionAsync();

            try
            {
                // 1. Đưa access token vào blacklist
                var blacklistResult = await _uow.AuthRepo.AddBlacklistedTokenAsync(accessToken);
                if (!blacklistResult.Succeeded)
                {
                    return blacklistResult;
                }

                // 2. Nếu có refresh token thì thu hồi
                if (!string.IsNullOrEmpty(refreshToken))
                {
                    var token = await _uow.AuthRepo.GetRefreshTokenAsync(refreshToken);

                    if (token != null)
                    {
                        await _uow.AuthRepo.RevokeRefreshTokenAsync(token);
                    }
                }

                // 3. Lưu tất cả thay đổi trong transaction
                await _uow.SaveChangesAsync();
                await transaction.CommitAsync();

                return OperationResult.Success();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error during logout for token: {Token}", accessToken);
                return OperationResult.Failed([$"Logout failed: {ex.Message}"]);
            }
        }
        public async Task<bool> IsAccessTokenBlacklistedAsync(string accessToken)
        {
            var check = await _uow.AuthRepo.IsTokenBlacklistedAsync(accessToken);
            return check;
        }

        public async Task<OperationResult> ResetPasswordAsync(Guid id, ResetPasswordRequest model)
        {
            try
            {
                // Validate input password
                if (string.IsNullOrWhiteSpace(model.NewPassword))
                {
                    _logger.LogWarning("New password is null or empty for user ID: {UserId}", id);
                    return OperationResult.Failed(["New password cannot be null or empty."]);
                }

                // Await the async method
                var user = await _userManager.FindByIdAsync(id.ToString());
                if (user == null)
                {
                    _logger.LogWarning("User not found with ID: {UserId}", id);
                    return OperationResult.Failed(["User not found."]);
                }

                // Option 1: Direct reset (admin function - bypasses current password)
                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, resetToken, model.NewPassword);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Password reset successfully for user ID: {UserId}", id);

                    // Optional: Update security stamp to invalidate existing tokens
                    await _userManager.UpdateSecurityStampAsync(user);

                    return OperationResult.Success();
                }
                else
                {
                    var errors = result.Errors.Select(e => e.Description).ToArray();
                    _logger.LogWarning("Password reset failed for user ID: {UserId}. Errors: {Errors}",
                                      id, string.Join(", ", errors));
                    return OperationResult.Failed(errors);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while resetting password for user ID: {UserId}", id);
                return OperationResult.Failed(["An error occurred while resetting password."]);
            }
        }

        /// <summary>
        /// Kiểm tra mã OTP (còn hạn và đúng) dựa trên email (hoặc userId) và mã pin.
        /// </summary>
        public async Task<bool> VerifyOtpAsync(string email, string pin)
        {
            // 1. Lấy user theo email
            var user = await _uow.UserRepo.GetUserByEmailAsync(email);
            if (user == null)
                throw new InvalidOperationException("Email does not exist.");

            // 2. Lấy OTP còn hiệu lực
            var otp = await _uow.OtpValidRepo.GetValidOtpAsync(user.Id, pin);
            if (otp == null)
                return false;

            // 3. (Tùy chọn) Xoá OTP sau khi sử dụng hoặc đánh dấu đã dùng
            //    Ví dụ: xoá luôn để tránh dùng lại
            await _uow.OtpValidRepo.RemoveByIdAsync(otp.Id);
            await _uow.SaveChangesAsync();
            var checkOtp = await _uow.OtpValidRepo.GetValidOtpAsync(user.Id, pin);
            if (checkOtp == null) return true;
            else return false;
        }
    }
}
