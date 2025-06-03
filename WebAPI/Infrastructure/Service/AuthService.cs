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

        public AuthService(
            IUnitOfWork uow,
            IJwtTokenGenerator tokenGen,
            IConfiguration configuration)
        {
            _uow = uow;
            _tokenGen = tokenGen;
            _configuration = configuration;
        }

        public async Task<RegisterResponse> RegisterAsync(RegisterRequest req, CancellationToken ct = default)
        {
            // 1. Sinh GUID cho user
            var newUserId = Guid.NewGuid();
            // 2. Map từ DTO → DomainUser (Identity) và AppUser (business)
            var domainUser = req.ToDomainUser(newUserId);
            var appUser = req.ToAppUser(newUserId);

            // 3. Bắt đầu transaction
            await using var tx = await _uow.BeginTransactionAsync(ct);
            try
            {
                // 4. Tạo ApplicationUser (AspNetUsers) từ DomainUser
                var createResult = await _uow.Auth.CreateUserAsync(domainUser, req.Password, ct);
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
                    AppRoleNames.Staff,
                    AppRoleNames.Member,
                    AppRoleNames.Guest
                };
                var matchedRole = validRoles.FirstOrDefault(r => string.Equals(r, req.Role?.Trim(), StringComparison.OrdinalIgnoreCase)) ??AppRoleNames.Member;

                if (!validRoles.Contains(matchedRole))
                {
                    throw new InvalidOperationException(
                        $"Role '{req.Role}' Invalid. Only accepted: {string.Join(", ", validRoles)}");
                }

                // 6. Gán role cho user
                var addRoleResult = await _uow.Auth.AddUserToRoleAsync(newUserId, matchedRole, ct);
                if (!addRoleResult.Succeeded)
                {
                    var roleErrors = string.Join("; ", addRoleResult.Errors);
                    throw new InvalidOperationException($"Cannot assign role: {roleErrors}");
                }

                // 7. Thêm AppUser (business)
                await _uow.Users.AddAsync(appUser);

                // 8. Lưu tất cả thay đổi: 
                // - Tất cả SaveChanges của CreateUserAsync và AddToRoleAsync đều nằm trong cùng transaction.
                await _uow.SaveChangesAsync(ct);

                // 9. Commit transaction
                await tx.CommitAsync(ct);

                // 10. Map DomainUser + AppUser + role → RegisterResponse (dùng mapper)
                return domainUser.ToRegisterResponse(appUser, matchedRole);
            }
            catch
            {
                // Nếu có bất kỳ lỗi nào (tạo Identity, gán role, thêm AppUser, thêm RefreshToken, SaveChanges…)
                // thì rollback toàn bộ. Nhờ đó, dù CreateUserAsync đã gọi SaveChanges, nó vẫn rollback vì
                // đang nằm trong transaction chung.
                await tx.RollbackAsync(ct);
                throw;
            }
        }


        public async Task<LoginResponse> LoginAsync(LoginRequest req, CancellationToken ct = default)
        {
            // 1. Lấy DomainUser (Identity), AppUser (business) và roles
            // domainUser
            var (domainUser, appUser, roles) = await _uow.Auth.GetUserWithRolesAndProfileAsync(req.UserName, ct);

            if (domainUser == null || appUser == null)
                throw new InvalidOperationException("Account does not exist");

            // 2. Check mật khẩu
            var passwordValid = await _uow.Auth.CheckPasswordAsync(req.UserName, req.Password, ct);
            if (!passwordValid)
                throw new InvalidOperationException("Incorrect username or password");

            // 3. Sinh AccessToken (JWT) dựa trên chính DomainUser + roles
            var (accessToken, accessExpire) = await _tokenGen.GenerateAccessTokenAsync(domainUser, roles);

            // 4. Sinh RefreshToken mới (trả về chuỗi, expiration và entity)
            var (refreshTokenValue, refreshExpireAt, refreshTokenEntity) = await _tokenGen.GenerateRefreshTokenAsync(domainUser.Id);

            // 5. Lưu RefreshToken entity vào DB
            await _uow.Auth.AddRefreshTokenAsync(refreshTokenEntity, ct);
            await _uow.SaveChangesAsync(ct);

            // 6. Lấy role chính (nếu cần trả cho client)
            var roleName = roles.FirstOrDefault() ?? AppRoleNames.Guest;

            // 7. Trả về LoginResponse (DomainUser đã có sẵn, AppUser, role, accessToken, refreshToken)
            return domainUser.ToLoginResponse(
                appUser,
                roleName,
                accessToken,
                refreshTokenValue);
        }


        public async Task<AuthResponse> RefreshTokenAsync(string refreshToken, CancellationToken ct = default)
        {
            // 1. Lấy RefreshToken entity cũ
            var existing = await _uow.Auth.GetRefreshTokenAsync(refreshToken, ct);
            if (existing == null || existing.Revoked || existing.ExpiresAt < DateTime.UtcNow)
                throw new InvalidOperationException("Refresh token is invalid or expired");

            // 2. Lấy DomainUser + roles bằng UserId
            var (domainUser, roles) = await _uow.Auth.GetUserWithRolesAsyncById(existing.UserId, ct);
            if (domainUser == null)
                throw new InvalidOperationException("User does not exist");

            // 3. Sinh AccessToken mới
            var (newAccessToken, newAccessExpire) = await _tokenGen.GenerateAccessTokenAsync(domainUser, roles);

            // 4. Thu hồi RefreshToken cũ
            await _uow.Auth.RevokeRefreshTokenAsync(existing, ct);

            // 5. Sinh RefreshToken mới
            var (newRefreshValue, newRefreshExpireAt, newRefreshEntity) = await _tokenGen.GenerateRefreshTokenAsync(domainUser.Id);

            // 6. Lưu entity mới vào DB
            await _uow.Auth.AddRefreshTokenAsync(newRefreshEntity, ct);
            await _uow.SaveChangesAsync(ct);

            // 7. Trả về AuthResponse (bao gồm cả các expiration nếu bạn muốn)
            return new AuthResponse
            {
                AccessToken = newAccessToken,
                AccessTokenExpiration = newAccessExpire,
                RefreshToken = newRefreshValue,
                RefreshTokenExpiration = newRefreshExpireAt
            };
        }

    }
}
