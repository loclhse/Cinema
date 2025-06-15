using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Application.IServices;
using Application.ViewModel.Request;
using Application.ViewModel.Response;
using Domain.Enums;
using Application;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/seat-type-prices")]
    public class SeatTypePriceController : ControllerBase
    {
        private readonly ISeatTypePriceService _service;

        public SeatTypePriceController(ISeatTypePriceService service)
        {
            _service = service;
        }

        [HttpGet("GetAllSeatTypePrice")]
        public async Task<IActionResult> GetAllAsync()
        {
            var prices = await _service.GetAllAsync();
            return Ok(prices);
        }

        [HttpGet("{seatType}")]
        public async Task<IActionResult> GetBySeatTypeAsync(SeatTypes seatType)
        {
            var price = await _service.GetBySeatTypeAsync(seatType);
            if (price == null) return NotFound();
            return Ok(price);
        }

        [HttpPut("{seatType}")]
        public async Task<IActionResult> UpdateAsync(SeatTypes seatType, [FromBody] SeatTypePriceUpdateRequest request)
        {
            var updated = await _service.UpdateAsync(seatType, request);
            return Ok(updated);
        }
    }
}