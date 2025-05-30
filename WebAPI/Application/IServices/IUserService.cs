using Application.ViewModel;
using Domain.Entities;

namespace Application.IServices
{
    public interface IUserService
    {
        Task<User> CreateEmployeeAccountAsync(WriteEmloyeeAccount employeeAccount);
        Task<List<ReadEmployeeAccount>> GetAllEmployeeAccountsAsync();
        Task<ReadEmployeeAccount?> GetEmployeeAccountByIdAsync(int id);
        Task<User?> UpdateEmployeeAccountAsync(int id, WriteEmloyeeAccount employeeAccount);
        Task<bool> DeleteEmployeeAccountAsync(int id);
    }
}
