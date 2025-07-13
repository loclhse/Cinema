using Application.IServices;
using Application.ViewModel.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScoreItemController : ControllerBase
    {
        private readonly IScoreItemService _scoreItemService;
        public ScoreItemController(IScoreItemService scoreItemService)
        {
            _scoreItemService = scoreItemService;
        }
        [HttpPost("create")]
        public async Task<IActionResult> CreateNewItem([FromBody] ItemRequest request)
        {
            var response = await _scoreItemService.CreateNewItemAsync(request);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateItem(Guid id, [FromBody] ItemRequest request)
        {
            var response = await _scoreItemService.UpdateItemAsync(id, request);
            if(response.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound(response);
            }
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteItem(Guid id)
        {
            var response = await _scoreItemService.DeleteItemAsync(id);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound(response);
            }
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }
        [HttpGet("get/{id}")]
        public async Task<IActionResult> GetItemById(Guid id)
        {
            var response = await _scoreItemService.GetItemByIdAsync(id);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound(response);
            }
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }
        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllItems(int page = 1, int size = 10)
        {
            var response = await _scoreItemService.GetAllItemsAsync(page, size);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound(response);
            }
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }
    }
}
