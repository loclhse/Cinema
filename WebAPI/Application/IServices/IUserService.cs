using Application.ViewModel;
using Application.ViewModel.Request;
using Application.ViewModels;
using Domain.Entities;
using Microsoft.VisualBasic;

namespace Application.IServices
{
    public interface IUserService
    {
        Task<ApiResp> GetAllMembesAsync();
        Task<ApiResp> GetMemberByIdAsync(Guid id);
        Task<ApiResp> UpdateMemberAsync(Guid id, MemberUpdateResquest memberUpdateResquest);
        Task<ApiResp> GetAllEmployeesAsync();
        Task<ApiResp> GetEmployeeByIdAsync(Guid id);
        Task<ApiResp> UpdateEmployeeAsync(Guid id, EmployeeUpdateResquest employeeUpdateResquest);
        Task<ApiResp> DeleteAccountAsync(Guid id);
        Task<ApiResp> SearchEmployeeAsync(string searchValue, SearchKey searchKey);
        Task<ApiResp> SearchMemberAsync(string searchValue , SearchKey searchKey);
  

        public enum SearchKey
        {
            Identitycart,
            PhoneNumeber,
            Name,
        }
    }
}
