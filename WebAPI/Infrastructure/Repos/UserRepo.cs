using Application.Domain;
using Application.IRepos;
using Domain.Entities;
using Infrastructure.Identity;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Repos
{
    public class UserRepo : GenericRepo<AppUser>, IUserRepo
    {
        private new readonly AppDbContext _db;
        public UserRepo(AppDbContext ctx) : base(ctx)
        {
            _db = ctx;
        }

        /* PRIVATE: lấy RoleId */
        private Task<Guid?> GetRoleIdAsync(string roleName) =>
            _db.Roles.Where(r => r.Name == roleName)
                     .Select(r => (Guid?)r.Id)   // IdentityRole<Guid>  
                     .FirstOrDefaultAsync();

        /*1. Projection: DomainUser */
        public async Task<List<DomainUser>> GetIdentityUsersByRoleAsync(string roleName)
        {
            var roleId = await GetRoleIdAsync(roleName);
            if (roleId == null) return new();

            return await _db.Users
                            .Where(u => _db.UserRoles
                                           .Any(ur => ur.UserId == u.Id &&
                                                      ur.RoleId == roleId))
                            .Select(u => new DomainUser
                            {
                                Id = u.Id,
                                UserName = u.UserName!,
                                Email = u.Email!,
                                Phone = u.PhoneNumber
                            })
                            .ToListAsync();
        }

        /* 2. Danh sách AppUser theo role */
        public async Task<IEnumerable<AppUser>> GetAllEmployeeAccountsAsync() =>
            await GetAllByRoleAsync(AppRoleNames.Employee);

        public async Task<IEnumerable<AppUser>> GetAllMemberAccountsAsync() =>
            await GetAllByRoleAsync(AppRoleNames.Member);

        public async Task<IEnumerable<AppUser>> GetAllCustomerAccountsAsync() =>
            await GetAllByRoleAsync(AppRoleNames.Customer);

        private async Task<IEnumerable<AppUser>> GetAllByRoleAsync(string roleName)
        {
            var roleId = await GetRoleIdAsync(roleName);
            if (roleId == null) return Enumerable.Empty<AppUser>();

            return await _db.Users
                            .Include(u => u.AppUser)
                            .Where(u => u.AppUser != null &&
                                        !u.AppUser.IsDeleted &&
                                        _db.UserRoles.Any(ur => ur.UserId == u.Id &&
                                                                ur.RoleId == roleId))
                            .Select(u => u.AppUser!)
                            .ToListAsync();
        }

        /* ---------- 3. Lấy 1 AppUser theo Id & role ---------- */
        public Task<AppUser?> GetEmployeeAccountAsync(Guid id) =>
            GetByRoleAndIdAsync(id, AppRoleNames.Employee);

        public Task<AppUser?> GetMemberAccountAsync(Guid id) =>
            GetByRoleAndIdAsync(id, AppRoleNames.Member);

        private async Task<AppUser?> GetByRoleAndIdAsync(Guid id, string roleName)
        {
            var roleId = await GetRoleIdAsync(roleName);
            if (roleId == null) return null;

            return await _db.Users
                            .Include(u => u.AppUser)
                            .Where(u => u.AppUser != null &&
                                        u.AppUser.Id == id &&
                                        !u.AppUser.IsDeleted &&
                                        _db.UserRoles.Any(ur => ur.UserId == u.Id &&
                                                                ur.RoleId == roleId))
                            .Select(u => u.AppUser!)
                            .FirstOrDefaultAsync();
        }

        /* 4. Check email */
        public async Task<bool> IsEmailExists(string email) =>
            await _db.Users
                     .Include(u => u.AppUser)
                     .AnyAsync(u => u.Email == email &&
                                    u.AppUser != null &&
                                    !u.AppUser.IsDeleted);
    }
}
