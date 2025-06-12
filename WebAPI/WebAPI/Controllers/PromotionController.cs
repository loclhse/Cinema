using Application.IServices;
using Application.Services;
using Application.ViewModel.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PromotionController : ControllerBase
    {
        private readonly IPromotionService _service;

        public PromotionController(IPromotionService promotionService)
        {
            _service = promotionService;
        }

        [HttpGet("GetAllPromotion")]
        public async Task<IActionResult> GetAllPromotion()
        {
            var rs = await _service.GetAllPromotion();
            return rs.IsSuccess ? Ok(rs) : NotFound(rs);
        }

        [HttpGet("GetPromomtionById")]
        public async Task<IActionResult> GetPromotionById(Guid Id)
        {
            var rs = await _service.GetPromotionById(Id);
            return rs.IsSuccess ? Ok(rs) : NotFound(rs);
        }

        //[Authorize(Roles = "Admin")]
        [HttpPost("AddPromotion")]
        public async Task<IActionResult> AddPromotion(EditPromotionRequest req)
        {
            var result = await _service.AddPromotion(req);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        //[Authorize(Roles = "Admin")]
        [HttpDelete("DeletePromotion")]
        public async Task<IActionResult> DeletePromotion(Guid id)
        {
            var result = await _service.DeletePromotion(id);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        [HttpPut("EditPromotion")]
        public async Task<IActionResult> EditPromotion(Guid id, [FromBody] EditPromotionRequest req)
        {
            var result = await _service.EditPromotion(id, req);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }
    }
}
