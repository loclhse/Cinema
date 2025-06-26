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

        [HttpPost("CreateTicketOrder")]
        public async Task<IActionResult> CreateOrder([FromBody] OrderRequest rq)
        {
            var rs = await _service.CreateTicketOrder(rq);
            return rs.IsSuccess ? Ok(rs) : NotFound(rs);
        }
    }
}
