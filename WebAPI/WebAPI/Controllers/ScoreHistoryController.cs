using Application.IServices;
using Application.ViewModel.Request;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScoreHistoryController : ControllerBase
    {
        private readonly IScoreHistoryService _scoreHistory;

        public ScoreHistoryController(IScoreHistoryService scoreHistoryService)
        {
            _scoreHistory = scoreHistoryService;
        }

        [HttpGet("View_Score_History")]
        public async Task<IActionResult> ViewScoreHistory(Guid userId)
        {
            var rs = await _scoreHistory.ViewHistory(userId);
            if (rs.StatusCode == HttpStatusCode.BadRequest)
            {
                return BadRequest(rs);
            }
            return rs.IsSuccess ? Ok(rs) : NotFound(rs);
        }

        [HttpDelete("Delete_Score_History")]
        public async Task<IActionResult> DeleteScoreHistory(Guid userId, Guid scoreId)
        {
            var rs = await _scoreHistory.DeleteHistory(userId, scoreId);
            if (rs.StatusCode == HttpStatusCode.BadRequest)
            {
                return BadRequest(rs);
            }
            return rs.IsSuccess ? Ok(rs) : NotFound(rs);
        }
    }
}
