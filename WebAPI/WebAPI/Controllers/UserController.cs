using Application.IServices;
using Application.ViewModel.Request;
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
        [HttpGet("GetAllMember")]
        public async Task<IActionResult> GetAllMember()
        {
            var result = await _userService.GetAllMembesAsync();
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        [HttpGet("GetMemberById/{id}")]
        public async Task<IActionResult> GetMemberById(Guid id)
        {
            var result = await _userService.GetMemberByIdAsync(id);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        [HttpPut("UpdateMember/{id}")]
        public async Task<IActionResult> UpdateMember(Guid id, MemberUpdateResquest request)
        {
            var result = await _userService.UpdateMemberAsync(id, request);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        [HttpDelete("DeleteMember/{id}")]
        public async Task<IActionResult> DeleteMember(Guid id)
        {
            var result = await _userService.DeleteMemberAsync(id);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }


        [HttpGet("SearchMember")]
        public async Task<IActionResult> SearchMember(string searchValue, SearchKey searchKey)
        {
            var result = await _userService.SearchMemberAsync(searchValue, searchKey);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

    }
}
