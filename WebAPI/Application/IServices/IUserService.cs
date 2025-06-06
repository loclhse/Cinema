using Application.Domain;
using Application.ViewModel;
using Application.ViewModel.Request;
using Application.ViewModels;
using Domain.Entities;
using Microsoft.VisualBasic;

namespace Application.IServices
{
    public interface IUserService
    {
        Task<ApiResp> GetAllCustomersAsync();
        Task<ApiResp> GetCustomerByIdAsync(Guid id);
        Task<ApiResp> UpdateCustomerAsync(Guid id, MemberUpdateResquest memberUpdateResquest);
        Task<ApiResp> DeleteCustomerAsync(Guid id);
        Task<ApiResp> GetAllEmployeesAsync();
        Task<ApiResp> GetEmployeeByIdAsync(Guid id);
        Task<ApiResp> UpdateEmployeeAsync(Guid id, EmployeeUpdateResquest employeeUpdateResquest);
        Task<ApiResp> DeleteEmployeeAsync(Guid id);
        Task<ApiResp> SearchCustomers(string value, SearchKey searchKey);
        Task<ApiResp> SearchEmployees(string value, SearchKey searchKey);
        Task<ApiResp> GetDeletedAccountsAsync();
        Task<ApiResp> RestoreAccountAsync(Guid id);
        Task<ApiResp> SearchIsDeleteEmployees(string value, SearchKey searchKey);


        public enum SearchKey
        {
            IdentityCard,
            PhoneNumeber,
            Name,
        }

    }
}


