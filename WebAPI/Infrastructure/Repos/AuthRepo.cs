using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using Application.Domain;
using Application.IRepos;
using Domain.Entities;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repos
{
    /// <summary>
    /// AuthRepo implement IAuthRepo:  
    /// - Chuyển DomainUser → ApplicationUser để thao tác Identity (UserManager).  
    /// - Chuyển IdentityResult → OperationResult, ApplicationUser → DomainUser.  
    /// - Quản lý RefreshToken (Domain.Entities.RefreshToken) qua AppDbContext.
    /// </summary>
    public class AuthRepo : IAuthRepo
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _dbContext;
        private readonly ILogger<AuthRepo> _logger;

        public AuthRepo(UserManager<ApplicationUser> userManager, AppDbContext dbContext, ILogger<AuthRepo> logger)
        {
            _userManager = userManager;
            _dbContext = dbContext;
            _logger = logger;
        }

        #region User (DomainUser ↔ ApplicationUser) Methods

        public async Task<OperationResult> CreateUserAsync(DomainUser user, string password)
        {
            // 1. Map DomainUser → ApplicationUser
            var identityUser = new ApplicationUser
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.Phone
            };

            // 2. Tạo user trong AspNetUsers
            var result = await _userManager.CreateAsync(identityUser, password);
            if (!result.Succeeded)
            {
                // Chuyển IdentityError => OperationResult.Errors (string[])
                var errors = result.Errors.Select(e => e.Description);
                return OperationResult.Failed(errors);
            }

            return OperationResult.Success();
        }

        public async Task<OperationResult> AddUserToRoleAsync(Guid userId, string roleName)
        {
            // 1. Tìm ApplicationUser theo Id
            var identityUser = await _userManager.FindByIdAsync(userId.ToString());
            if (identityUser == null)
                return OperationResult.Failed(["User not found."]);
            // 2. Thêm role cho user
            var result = await _userManager.AddToRoleAsync(identityUser, roleName);
            if (!result.Succeeded)
            {
                // Chuyển IdentityError => OperationResult.Errors (string[])
                var errors = result.Errors.Select(e => e.Description);
                return OperationResult.Failed(errors);
            }
            return OperationResult.Success();
        }

        public async Task<(DomainUser? user, string[] roles)> GetUserWithRolesAsync(string userName)
        {
            // 1. Tìm ApplicationUser theo UserName
            var identityUser = await _userManager.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserName == userName);

            if (identityUser == null)
                return (null, Array.Empty<string>());

            // 2. Lấy roles
            var roles = (await _userManager.GetRolesAsync(identityUser)).ToArray();

            // 3. Map ApplicationUser → DomainUser
            var domainUser = new DomainUser
            {
                Id = identityUser.Id,
                UserName = identityUser.UserName ?? string.Empty,
                Email = identityUser.Email ?? string.Empty,
                Phone = identityUser.PhoneNumber
            };

            return (domainUser, roles);
        }

        public async Task<(DomainUser? user, string[] roles)> GetUserWithRolesAsyncById(Guid userId)
        {
            // 1. Tìm ApplicationUser theo Id
            var identityUser = await _userManager.FindByIdAsync(userId.ToString());
            if (identityUser == null)
                return (null, Array.Empty<string>());

            // 2. Lấy roles
            var roles = (await _userManager.GetRolesAsync(identityUser)).ToArray();

            // 3. Map ApplicationUser → DomainUser
            var domainUser = new DomainUser
            {
                Id = identityUser.Id,
                UserName = identityUser.UserName ?? string.Empty,
                Email = identityUser.Email ?? string.Empty,
                Phone = identityUser.PhoneNumber
            };

            return (domainUser, roles);
        }

        public async Task<bool> CheckPasswordAsync(string userName, string password)
        {
            // 1. Tìm ApplicationUser theo UserName
            var identityUser = await _userManager.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserName == userName);

            if (identityUser == null)
                return false;

            // 2. Kiểm tra password
            return await _userManager.CheckPasswordAsync(identityUser, password);
        }

        public async Task<OperationResult> ChangePasswordAsync(Guid id, string oldPassword, string newPassword)
        {
            try
            {
                // 1. Find user by ID (no need for AsNoTracking since we're updating)
                var user = await _userManager.FindByIdAsync(id.ToString());
                if (user == null)
                {
                    _logger.LogWarning("User with ID {UserId} not found", id);
                    return OperationResult.Failed(["User not found"]);
                }

                // 2. Change password
                var result = await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);

                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToArray();
                    _logger.LogWarning("Password change failed for user {UserId}. Errors: {Errors}",
                        id, string.Join(", ", errors));
                    return OperationResult.Failed(errors);
                }

                _logger.LogInformation("Password changed successfully for user {UserId}", id);
                return OperationResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user {UserId}", id);
                return OperationResult.Failed(["An unexpected error occurred while changing password"]);
            }
        }

        public async Task<(DomainUser? domainUser, AppUser? appUser, List<string> roles)> GetUserWithRolesAndProfileAsync(string userName)
        {
            // 1. Lấy ApplicationUser (Identity) kèm AppUser (profile) từ DB
            var identityUser = await _userManager.Users
                .Include(u => u.AppUser)
                .FirstOrDefaultAsync(u => u.UserName == userName);

            if (identityUser == null)
                throw new InvalidOperationException("User not found");

            // 2. Lấy roles
            var roles = (await _userManager.GetRolesAsync(identityUser)).ToList();

            // 3. Map ApplicationUser → DomainUser
            var domainUser = new DomainUser
            {
                Id = identityUser.Id,
                UserName = identityUser.UserName ?? string.Empty,
                Email = identityUser.Email ?? string.Empty,
                Phone = identityUser.PhoneNumber
            };

            // 4. AppUser đã include ở bước 1; có thể null nếu chưa có
            var appUser = identityUser.AppUser;

            return (domainUser, appUser, roles);
        }

        public async Task<(DomainUser? domainUser, AppUser? appUser, List<string> roles)> GetUserWithRolesAndProfileByIdAsync(Guid userId)
        {
            var identityUser = await _userManager.Users
                .Include(u => u.AppUser)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (identityUser == null)
                throw new InvalidOperationException("User not found");

            var roles = (await _userManager.GetRolesAsync(identityUser)).ToList();

            var domainUser = new DomainUser
            {
                Id = identityUser.Id,
                UserName = identityUser.UserName ?? string.Empty,
                Email = identityUser.Email ?? string.Empty,
                Phone = identityUser.PhoneNumber
            };

            var appUser = identityUser.AppUser;
            return (domainUser, appUser, roles);
        }

        #endregion

        #region RefreshToken (Domain) Methods

        public async Task AddRefreshTokenAsync(RefreshToken refreshToken)
        {
            await _dbContext.RefreshTokens.AddAsync(refreshToken);
        }

        public Task<RefreshToken?> GetRefreshTokenAsync(string token)
        {
            return _dbContext.RefreshTokens
                             .AsNoTracking()
                             .FirstOrDefaultAsync(rt => rt.Token == token)!;
        }

        public Task RevokeRefreshTokenAsync(RefreshToken token)
        {
            token.RevokedAt = DateTime.UtcNow;
            _dbContext.RefreshTokens.Update(token);
            return Task.CompletedTask;
        }

        public async Task<OperationResult> AddBlacklistedTokenAsync(string token)
        {
            try
            {
                // Kiểm tra xem token đã tồn tại chưa
                var existingBlacklistedToken = await _dbContext.BlacklistedTokens
                    .FirstOrDefaultAsync(bt => bt.Token == token);

                if (existingBlacklistedToken != null)
                {
                    // Token đã ở trong blacklist, coi như thành công
                    return OperationResult.Success();
                }

                var blacklistedToken = new BlacklistedToken
                {
                    Id = Guid.NewGuid(),
                    Token = token,
                    BlacklistedAt = DateTime.UtcNow
                };

                await _dbContext.BlacklistedTokens.AddAsync(blacklistedToken);
                return OperationResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding blacklisted token: {Token}", token);
                return OperationResult.Failed(["Failed to add token to blacklist"]);
            }
        }

        public async Task<bool> IsTokenBlacklistedAsync(string token)
        {
            return await _dbContext.BlacklistedTokens
                .AsNoTracking()
                .AnyAsync(t => t.Token == token);
        }
        #endregion
    }
}

