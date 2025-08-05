using Application.IServices;
using Application.ViewModel.Request;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _service;

        public OrderController(IOrderService service)
        {
            _service = service;
        }

        [HttpPost("PendingTicketOrder")]
        public async Task<IActionResult> CreateOrder([FromBody] OrderRequest rq)
        {
            var rs = await _service.CreateTicketOrder(rq);
            if(rs.StatusCode == HttpStatusCode.BadRequest)
            {
                return BadRequest(rs);
            }
            return rs.IsSuccess ? Ok(rs) : NotFound(rs);
        }

        [HttpGet("GetAllTicketOrder")]
        public async Task<IActionResult> GetAllTicketOrder()
        {
            var rs = await _service.ViewTicketOrder();
            if (rs.StatusCode == HttpStatusCode.BadRequest)
            {
                return BadRequest(rs);
            }
            return rs.IsSuccess ? Ok(rs) : NotFound(rs);
        }

        [HttpGet("ViewTicketByUserId")]
        public async Task<IActionResult> GetTicketByUserId(Guid userId)
        {
            var rs = await _service.ViewTicketOrderByUserId(userId);
            if (rs.StatusCode == HttpStatusCode.BadRequest)
            {
                return BadRequest(rs);
            }
            return rs.IsSuccess ? Ok(rs) : NotFound(rs);
        }

        [HttpPut("CancelOrder")]
        public async Task<IActionResult> CancelOrder(List<Guid> seatScheduleId, Guid orderId)
        {
            var rs = await _service.CancelTicketOrderById(seatScheduleId, orderId);
            if (rs.StatusCode == HttpStatusCode.BadRequest)
            {
                return BadRequest(rs);
            }
            return rs.IsSuccess ? Ok(rs) : NotFound(rs);
        }

        [HttpPut("SuccessOrder")]
        public async Task<IActionResult> SuccessOrder(List<Guid> seatScheduleId, Guid orderId, Guid userId, Guid? movieId)
        {
            var rs = await _service.SuccessOrder(seatScheduleId, orderId, userId, movieId);
            if (rs.StatusCode == HttpStatusCode.BadRequest)
            {
                return BadRequest(rs);
            }
            return rs.IsSuccess ? Ok(rs) : NotFound(rs);
        }
    }
}
