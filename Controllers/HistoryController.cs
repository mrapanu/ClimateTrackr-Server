using ClimateTrackr_Server.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClimateTrackr_Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HistoryController : ControllerBase
    {
        private readonly DataContext _context;
        public HistoryController(DataContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("GetHistory")]
        public async Task<ActionResult<ServiceResponse<List<GetHistoryDto>>>> GetHistory()
        {
            var response = new ServiceResponse<List<GetHistoryDto>>();
            var historyData = await _context.History.OrderByDescending(h => h.DateTime).Take(200).ToListAsync();
            if(historyData.Count == 0)
            {
                response.Message = "There are no entries in history.";
                response.Success = false;
                return Ok(response);
            }

            response.Data = historyData.Select(h => new GetHistoryDto
            {
                ActionMessage = h.ActionMessage,
                DateTime = h.DateTime,
                User = h.User
            }).ToList();
            response.Message = "Successfully get data from history.";
            response.Success = true;
            return Ok(response);
        }
    }
}