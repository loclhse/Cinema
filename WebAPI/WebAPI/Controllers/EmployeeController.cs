using Application.Domain;
using Application.IServices;
using Application.ViewModel.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static Application.IServices.IUserService;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly IUserService _userService;

        public EmployeeController(IUserService userService)
        {
            _userService = userService;
        }
        [HttpGet("GetAllEmployee")]
        public async Task<IActionResult> GetAllEmployee()
        {
            var result = await _userService.GetAllEmployeesAsync();
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        [HttpGet("GetEmployeeById/{id}")]
        public async Task<IActionResult> GetEmployeeById(Guid id)
        {
            var result = await _userService.GetEmployeeByIdAsync(id);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        [HttpPut("UpdateEmployee/{id}")]
        public async Task<IActionResult> UpdateEmployee(Guid id, EmployeeUpdateResquest request)
        {
            var result = await _userService.UpdateEmployeeAsync(id, request);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }
        [HttpDelete("DeleteEmployee/{id}")]
        public async Task<IActionResult> DeleteEmployee(Guid id)
        {
            var result = await _userService.DeleteEmployeeAsync(id);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }
        [HttpGet("SearchEmployee")]
        public async Task<IActionResult> SearchEmployee(string value, SearchKey searchKey)
        {
            var result = await _userService.SearchEmployees(value, searchKey);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        [HttpGet("GetDeletedAccounts")]
        public async Task<IActionResult> GetDeletedAccounts()
        {
            var result = await _userService.GetDeletedAccountsAsync();
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        [HttpPut("RestoreAccount/{id}")]
        public async Task<IActionResult> RestoreAccount(Guid id)
        {
            var result = await _userService.RestoreAccountAsync(id);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }
    }
}
