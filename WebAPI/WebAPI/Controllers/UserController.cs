using Application.IServices;
using Application.ViewModel.Request;
using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using System.Net.WebSockets;
using static Application.IServices.IUserService;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }
        [HttpGet("GetAllCustomers")]
        public async Task<IActionResult> GetAllCustomers()
        {
            var result = await _userService.GetAllCustomersAsync();
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        [HttpGet("GetCustomerById/{id}")]
        public async Task<IActionResult> GetCustomerById(Guid id)
        {
            var result = await _userService.GetCustomerByIdAsync(id);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        [HttpPut("UpdateCustomer/{id}")]
        public async Task<IActionResult> UpdateCustomer(Guid id, MemberUpdateResquest request)
        {
            var result = await _userService.UpdateCustomerAsync(id, request);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        [HttpDelete("DeleteCustomer/{id}")]
        public async Task<IActionResult> DeleteCustomer(Guid id)
        {
            var result = await _userService.DeleteCustomerAsync(id);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }
        [HttpGet("SearchCustomer")]
        public async Task<IActionResult> SearchCustomer(string value, SearchKey searchKey)
        {
            var result = await _userService.SearchCustomers(value, searchKey);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }
        
    }
}
