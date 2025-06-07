using Application.IServices;
using Microsoft.AspNetCore.Mvc;

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

        [HttpGet("GetAllMember")]
        public async Task<IActionResult> getAllMember()
        {
            var rs = await _memberService.GetAllMember();
            return rs.IsSuccess ? Ok(rs) : BadRequest();
        }
    }
}
