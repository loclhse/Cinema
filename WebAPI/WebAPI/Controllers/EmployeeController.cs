using Application.Domain;
using Application.IServices;
using Application.ViewModel.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using static Application.IServices.IUserService;
using static Google.Apis.Requests.BatchRequest;

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
        [Authorize(Roles = "Admin")]
        [HttpGet("GetAllEmployee")]
        public async Task<IActionResult> GetAllEmployee()
        {
            var result = await _userService.GetAllEmployeesAsync();
            if (result.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound();
            }
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }


        [HttpGet("GetEmployeeById/{id}")]
        public async Task<IActionResult> GetEmployeeById(Guid id)
        {
            var result = await _userService.GetEmployeeByIdAsync(id);
            if (result.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound();
            }
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPut("UpdateEmployee/{id}")]
        public async Task<IActionResult> UpdateEmployee(Guid id, EmployeeUpdateResquest request)
        {
            var result = await _userService.UpdateEmployeeAsync(id, request);
            if (result.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound();
            }
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("DeleteEmployee/{id}")]
        public async Task<IActionResult> DeleteEmployee(Guid id)
        {
            var result = await _userService.DeleteEmployeeAsync(id);
            if (result.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound();
            }
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("SearchEmployee")]
        public async Task<IActionResult> SearchEmployee(string value, SearchKey searchKey)
        {
            var result = await _userService.SearchEmployees(value, searchKey);
            if (result.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound();
            }
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("GetAllDeletedAccounts")]
        public async Task<IActionResult> GetDeletedAccounts()
        {
            var result = await _userService.GetDeletedAccountsAsync();
            if (result.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound();
            }
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("RestoreAccount/{id}")]
        public async Task<IActionResult> RestoreAccount(Guid id)
        {
            var result = await _userService.RestoreAccountAsync(id);
            if (result.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound();
            }
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("SearchIsDeleteEmployee")]
        public async Task<IActionResult> SearchIsDeleteEmployee(string value, SearchKey searchKey)
        {
            var result = await _userService.SearchIsDeleteEmployees(value, searchKey);
            if (result.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound();
            }
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}
