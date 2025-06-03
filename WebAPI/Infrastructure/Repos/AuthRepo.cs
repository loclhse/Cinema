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

        public AuthRepo(UserManager<ApplicationUser> userManager, AppDbContext dbContext)
        {
            _userManager = userManager;
            _dbContext = dbContext;
        }

        #region User (DomainUser ↔ ApplicationUser) Methods

        public async Task<OperationResult> CreateUserAsync(DomainUser user, string password, CancellationToken ct = default)
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

        public async Task<OperationResult> AddUserToRoleAsync(Guid userId, string roleName, CancellationToken ct = default)
        {
            // 1. Tìm ApplicationUser theo Id
            var identityUser = await _userManager.FindByIdAsync(userId.ToString());
            if (identityUser == null)
                return OperationResult.Failed(new[] { "User not found." });
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

        public async Task<(DomainUser? user, string[] roles)> GetUserWithRolesAsync(string userName, CancellationToken ct = default)
        {
            // 1. Tìm ApplicationUser theo UserName
            var identityUser = await _userManager.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserName == userName, ct);

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

        public async Task<(DomainUser? user, string[] roles)> GetUserWithRolesAsyncById(Guid userId, CancellationToken ct = default)
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

        public async Task<bool> CheckPasswordAsync(string userName, string password, CancellationToken ct = default)
        {
            // 1. Tìm ApplicationUser theo UserName
            var identityUser = await _userManager.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserName == userName, ct);

            if (identityUser == null)
                return false;

            // 2. Kiểm tra password
            return await _userManager.CheckPasswordAsync(identityUser, password);
        }

        public async Task<(DomainUser? domainUser, AppUser? appUser, List<string> roles)> GetUserWithRolesAndProfileAsync(string userName, CancellationToken ct)
        {
            // 1. Lấy ApplicationUser (Identity) kèm AppUser (profile) từ DB
            var identityUser = await _userManager.Users
                .Include(u => u.AppUser)
                .FirstOrDefaultAsync(u => u.UserName == userName, cancellationToken: ct);

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

        public async Task<(DomainUser? domainUser, AppUser? appUser, List<string> roles)> GetUserWithRolesAndProfileByIdAsync(Guid userId, CancellationToken ct)
        {
            var identityUser = await _userManager.Users
                .Include(u => u.AppUser)
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken: ct);

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

        public async Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken ct = default)
        {
            await _dbContext.RefreshTokens.AddAsync(refreshToken, ct);
        }

        public Task<RefreshToken?> GetRefreshTokenAsync(string token, CancellationToken ct = default)
        {
            return _dbContext.RefreshTokens
                             .AsNoTracking()
                             .FirstOrDefaultAsync(rt => rt.Token == token, ct)!;
        }

        public Task RevokeRefreshTokenAsync(RefreshToken token, CancellationToken ct = default)
        {
            token.RevokedAt = DateTime.UtcNow;
            _dbContext.RefreshTokens.Update(token);
            return Task.CompletedTask;
        }

        #endregion
    }
}

