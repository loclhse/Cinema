using Application.IServices;
using Application.ViewModel.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using static Application.IServices.IUserService;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MemberController : ControllerBase
    {
        private readonly IMemberService _memberService;

        public MemberController(IMemberService memberService)
        {
            _memberService = memberService;
        }

        [Authorize(Roles = "Employee,Admin")]
        [HttpGet("GetAllMember")]
        public async Task<IActionResult> getAllMember()
        {
            var rs = await _memberService.GetAllMember();
            if (rs.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound();
            }
            return rs.IsSuccess ? Ok(rs) : BadRequest();
        }
        [Authorize(Roles = "Employee,Admin")]
        [HttpGet("SearchMember")]
        public async Task<IActionResult> SearchMember(string value, SearchKey searchKey)
        {
            var rs = await _memberService.SearchMembers(value, searchKey);
            if (rs.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound();
            }
            return rs.IsSuccess ? Ok(rs) : BadRequest();
        }

        [Authorize(Roles = "Employee,Admin")]
        [HttpDelete("DeleteMember/{id}")]
        public async Task<IActionResult> DeleteMember(Guid id)
        {
            var rs = await _memberService.DeleteMemberAsync(id);
            if (rs.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound();
            }
            return rs.IsSuccess ? Ok(rs) : BadRequest(rs);
        }
        [Authorize(Roles = "Employee,Admin")]
        [HttpPut("UpdateMember/{id}")]
        public async Task<IActionResult> UpdateMember(Guid id, [FromBody] CustomerUpdateResquest req)
        {
            if (req == null)
            {
                return BadRequest("Request body cannot be null.");
            }
            var rs = await _memberService.UpdateMemberAsync(id, req);
            if (rs.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound();
            }
            return rs.IsSuccess ? Ok(rs) : BadRequest(rs);
        }
    }
}
