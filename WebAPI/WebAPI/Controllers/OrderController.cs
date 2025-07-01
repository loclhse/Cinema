using Application.IServices;
using Application.ViewModel.Request;
using Microsoft.AspNetCore.Mvc;

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
            return rs.IsSuccess ? Ok(rs) : NotFound(rs);
        }

        [HttpGet("GetAllTicketOrder")]
        public async Task<IActionResult> GetAllTicketOrder(int page, int size)
        {
            var rs = await _service.ViewTicketOrder(page, size);
            return rs.IsSuccess ? Ok(rs) : NotFound(rs);
        }

        [HttpGet("ViewTicketByUserId")]
        public async Task<IActionResult> GetTicketByUserId(Guid userId)
        {
            var rs = await _service.ViewTicketOrderByUserId(userId);
            return rs.IsSuccess ? Ok(rs) : NotFound(rs);
        }

        [HttpPut("CancelOrder")]
        public async Task<IActionResult> CancelOrder(List<Guid> seatScheduleId, Guid orderId)
        {
            var rs = await _service.CancelTicketOrderById(seatScheduleId, orderId);
            return rs.IsSuccess ? Ok(rs) : NotFound(rs);
        }

        [HttpPut("SuccessOrder")]
        public async Task<IActionResult> SuccessOrder(List<Guid> seatScheduleId, Guid orderId)
        {
            var rs = await _service.SuccessOrder(seatScheduleId, orderId);
            return rs.IsSuccess ? Ok(rs) : NotFound(rs);
        }
    }
}
