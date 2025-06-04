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
        Task<ApiResp> DeleteMemberAsync(Guid id);
        Task<ApiResp> GetAllEmployeesAsync();
        Task<ApiResp> GetEmployeeByIdAsync(Guid id);
        Task<ApiResp> UpdateEmployeeAsync(Guid id, EmployeeUpdateResquest employeeUpdateResquest);
        Task<ApiResp> DeleteEmployeeAsync(Guid id);
  

        public enum SearchKey
        {
            IdentityCard,
            PhoneNumeber,
            Name,
        }
    }
}
