using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Application.IServices;
using Domain.Entities;
using Domain.Enums;
using Application.ViewModel.Request;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/seats")]
    public class SeatController : ControllerBase
    {
        private readonly a _seatService;

        public SeatController(a seatService)
        {
            _seatService = seatService;
        }

        [HttpGet("room/{roomId}")]
        public async Task<IActionResult> GetSeatsByRoomAsync(Guid roomId)
        {
            var seats = await _seatService.GetSeatsByRoomAsync(roomId);
            return Ok(seats);
        }

        [HttpGet("{seatId}")]
        public async Task<IActionResult> GetSeatByIdAsync(Guid seatId)
        {
            var seat = await _seatService.GetSeatByIdAsync(seatId);
            if (seat == null) return NotFound();
            return Ok(seat);
        }

        [HttpPut("update-type")]
        public async Task<IActionResult> UpdateSeatType([FromBody] UpdateSeatTypeModel model)
        {
            await _seatService.UpdateSeatTypeAsync(model.SeatIds, model.NewType);
            return NoContent();
        }

        [HttpPut("update-availability")]
        public async Task<IActionResult> UpdateSeatAvailability([FromBody] UpdateSeatAvailabilityModel model)
        {
            await _seatService.UpdateSeatAvailabilityAsync(model.SeatIds, model.IsAvailable);
            return NoContent();
        }
    }
}