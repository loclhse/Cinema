using Application.IRepos;
using Domain.Entities;
using Infrastructure.Identity;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Infrastructure.Repos
{
    public class UserRepo : GenericRepo<AppUser>, IUserRepo
    {
        private readonly AppDbContext _appDbContext;

        public UserRepo(AppDbContext context) : base(context)
        {
            _appDbContext = context;
        }
        public async Task<int> IsEmployeeAccount(Guid id)
        {
            int result = 0;
            // 3. Kiểm tra xem ApplicationUser này có role "Staff" không
            var userRoles = await _appDbContext.UserRoles
                .Where(ur => ur.UserId == id)
                .Select(ur => ur.RoleId)
                .ToListAsync();

            var employeeRole = await _appDbContext.Roles
                .FirstOrDefaultAsync(r => r.Name == AppRoleNames.Employee);

            if (employeeRole == null || !userRoles.Contains(employeeRole.Id))
            {
                result = 0; // Không phải là nhân viên
            }
            result = 1; // Là nhân viên
            return result;
        }

        public async Task<int> IsCustomerAccount(Guid id)
        {
            int result = 0;
            // 3. Kiểm tra xem ApplicationUser này có role "Cus" không
            var userRoles = await _appDbContext.UserRoles
                .Where(ur => ur.UserId == id)
                .Select(ur => ur.RoleId)
                .ToListAsync();

            var CusRole = await _appDbContext.Roles
                .FirstOrDefaultAsync(r => r.Name == AppRoleNames.Customer);

            if (CusRole == null || !userRoles.Contains(CusRole.Id))
            {
                result = 0; // Không phải là cus
            }
            result = 1; // Là cus
            return result;
        }
        //public async Task<List<ApplicationUser>> GetAllIUsersByRoleAsync(string roleName)
        //{
        //    // 1. Tìm role trong AspNetRoles
        //    var role = await _appDbContext.Roles
        //        .FirstOrDefaultAsync(r => r.Name == roleName);

        //    if (role == null)
        //    {
        //        return new List<ApplicationUser>();
        //    }

        //    // 2. Lấy danh sách UserId của role đó
        //    var userIdsInRole = await _appDbContext.UserRoles
        //        .Where(ur => ur.RoleId == role.Id)
        //        .Select(ur => ur.UserId)
        //        .ToListAsync();

        //    if (!userIdsInRole.Any())
        //    {
        //        return new List<ApplicationUser>();
        //    }

        //    // 3. Lọc từ AspNetUsers, include AppUser (profile), đồng thời chỉ lấy những AppUser không null và IsDeleted==false
        //    var list = await _appDbContext.Users
        //        .Include(u => u.AppUser)
        //        .Where(u => userIdsInRole.Contains(u.Id)
        //                    && u.AppUser != null
        //                    && !u.AppUser.IsDeleted)
        //        .ToListAsync();

        //    return list;
        //}
        /// <summary>
        /// Lấy toàn bộ các tài khoản nhân viên (role = "Employee"), trả về list AppUser.
        /// </summary>
        public async Task<IEnumerable<AppUser>> GetAllEmployeeAccounts()
        {
            // 1. Tìm role "Staff"
            var employeeRole = await _appDbContext.Roles
                .FirstOrDefaultAsync(r => r.Name == AppRoleNames.Employee);

            if (employeeRole == null)
            {
                // Nếu không có role "Staff", trả về rỗng
                return Enumerable.Empty<AppUser>();
            }

            // 2. Lấy danh sách UserId có role = "Staff"
            var staffUserIds = await _appDbContext.UserRoles
                .Where(ur => ur.RoleId == employeeRole.Id)
                .Select(ur => ur.UserId)
                .ToListAsync();

            if (staffUserIds.Count == 0)
            {
                return Enumerable.Empty<AppUser>();
            }

            // 3. Lấy ApplicationUser kèm theo AppUser để select AppUser
            var appUsers = await _appDbContext.Users
                .Include(u => u.AppUser)
                .Where(u => staffUserIds.Contains(u.Id) && u.AppUser != null && !u.AppUser.IsDeleted)
                .Select(u => u.AppUser!)
                .ToListAsync();

            return appUsers;
        }

        /// <summary>
        /// Lấy 1 tài khoản nhân viên theo AppUser.Id.
        /// Nếu không tìm thấy hoặc không phải là Staff, ném exception.
        /// </summary>
        public async Task<AppUser?> GetEmployeeAccount(Guid id)
        {
            // 1. Kiểm tra AppUser có tồn tại và chưa bị xóa
            var appUser = await _appDbContext.AppUsers
                .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);

            if (appUser == null)
            {
                throw new Exception("Employee not found");
            }

            // 2. Tìm ApplicationUser có AppUser.Id == id
            var applicationUser = await _appDbContext.Users
                .Include(u => u.AppUser)
                .FirstOrDefaultAsync(u => u.AppUser != null && u.AppUser.Id == id);

            if (applicationUser == null)
            {
                throw new Exception("Employee not found");
            }

            // 3. Kiểm tra xem ApplicationUser này có role "Staff" không
            var userRoles = await _appDbContext.UserRoles
                .Where(ur => ur.UserId == applicationUser.Id)
                .Select(ur => ur.RoleId)
                .ToListAsync();

            var employeeRole = await _appDbContext.Roles
                .FirstOrDefaultAsync(r => r.Name == AppRoleNames.Employee);

            if (employeeRole == null || !userRoles.Contains(employeeRole.Id))
            {
                throw new Exception("Employee not found");
            }

            return appUser;
        }

        public Task<AppUser?> GetEmployeeAccount(int id)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Kiểm tra email đã tồn tại hay chưa (trên AspNetUsers), đồng thời AppUser.IsDeleted == false.
        /// </summary>
        public async Task<bool> IsEmailExists(string email)
        {
            // Kết hợp ApplicationUser và AppUser để kiểm tra IsDeleted
            return await _appDbContext.Users
                .Include(u => u.AppUser)
                .AnyAsync(u => u.Email == email
                               && u.AppUser != null
                               && !u.AppUser.IsDeleted);
        }
    }
}
