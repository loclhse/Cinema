using Application.ViewModel;

namespace Application.IServices
{
    public interface IEmployeeService
    {
        Task<ApiResp> GetAllEmployeesAsync();
        Task<ApiResp> GetEmployeeByIdAsync(Guid id);
        Task<ApiResp> UpdateEmployeeAsync(Guid id);
        Task<ApiResp> DeleteEmployeeAsync(Guid id);

    }
}
