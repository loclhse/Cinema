    using Application.Domain;      // DomainUser
using Domain.Entities;         // AppUser
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.IRepos
{
    public interface IUserRepo : IGenericRepo<AppUser>
    {
        /* Identity projections (DomainUser) */
        Task<List<DomainUser>> GetIdentityUsersByRoleAsync(string roleName);

        /* AppUser helpers */
        Task<IEnumerable<AppUser>> GetAllEmployeeAccountsAsync();
        Task<IEnumerable<AppUser>> GetAllMemberAccountsAsync();
        Task<IEnumerable<AppUser>> GetAllCustomerAccountsAsync();

        Task<IEnumerable<AppUser>> GetAllEmployeeAccountsDeletedAsync();
        Task<IEnumerable<AppUser>> GetAllMemberAccountsDeletedAsync();
        Task<IEnumerable<AppUser>> GetAllCustomerAccountsDeletedAsync();

        Task<AppUser?> GetDeletedEmployeeAccountAsync(Guid id);
        Task<AppUser?> GetEmployeeAccountAsync(Guid id);
        Task<AppUser?> GetMemberAccountAsync(Guid id);
        Task<AppUser?> GetCustomerAccountAsync(Guid id);

        Task<AppUser?> GetDeletedByRoleAndIdAsync(Guid id, string roleName);

        Task<bool> IsEmailExists(string email);
        Task<AppUser?> GetUserByEmailAsync(string toEmail);
    }
}
