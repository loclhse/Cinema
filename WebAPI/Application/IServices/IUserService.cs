using Application.ViewModel;
using Domain.Entities;

namespace Application.IServices
{
    public interface IUserService
    {
        //Task<AppUser> CreateEmployeeAccountAsync(WriteEmloyeeAccount employeeAccount);
        Task<List<ReadEmployeeAccount>> GetAllEmployeeAccountsAsync();
        Task<ReadEmployeeAccount?> GetEmployeeAccountByIdAsync(Guid id);
        //Task<AppUser?> UpdateEmployeeAccountAsync(Guid id, WriteEmloyeeAccount employeeAccount);
        //Task<bool> DeleteEmployeeAccountAsync(Guid id);
    }
}
