﻿using Application.IServices;
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

        [HttpGet]
        public async Task<IActionResult> GetAllPromotion()
        {
            var rs = await _service.GetAllPromotion();
            return rs.IsSuccess ? Ok(rs) : NotFound(rs);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPromotionById(Guid id)
        {
            var rs = await _service.GetPromotionById(id);
            return rs.IsSuccess ? Ok(rs) : NotFound(rs);
        }

        //[Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> AddPromotion(List<EditPromotionRequest> requests)
        {
            for (int i = 0; i < requests.Count; i++)
            {
                var result = await _service.AddPromotion(requests[i]);
                if (!result.IsSuccess)
                {
                    return BadRequest(result);
                }
            }
            return Ok(new Application.ViewModel.ApiResp().SetOk("Promotions added successfully."));
        }
        //[Authorize(Roles = "Admin")]
        [HttpDelete]
        public async Task<IActionResult> DeletePromotion(Guid id)
        {
            var result = await _service.DeletePromotion(id);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        [HttpPut]
        public async Task<IActionResult> EditPromotion(Guid id, [FromBody] EditPromotionRequest req)
        {
            var result = await _service.EditPromotion(id, req);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }
    }
}
