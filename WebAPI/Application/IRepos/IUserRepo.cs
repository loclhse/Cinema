using Application.Domain;      // DomainUser
using Domain.Entities;         // AppUser
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.IRepos
{
    public interface IUserRepo : IGenericRepo<AppUser>
    {
        /* ===== Identity projections (DomainUser) ===== */
        Task<List<DomainUser>> GetIdentityUsersByRoleAsync(string roleName);

        /* ===== AppUser helpers ===== */
        Task<IEnumerable<AppUser>> GetAllEmployeeAccountsAsync();
        Task<IEnumerable<AppUser>> GetAllMemberAccountsAsync();
        Task<IEnumerable<AppUser>> GetAllCustomerAccountsAsync();

        Task<AppUser?> GetEmployeeAccountAsync(Guid id);
        Task<AppUser?> GetMemberAccountAsync(Guid id);

        Task<bool> IsEmailExists(string email);
    }
}
