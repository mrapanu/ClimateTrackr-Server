using ClimateTrackr_Server.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClimateTrackr_Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TempAndHumController : ControllerBase
    {
        private readonly DataContext _context;

        public TempAndHumController(DataContext context)
        {
            _context = context;
        }

        [HttpGet("getbyroom")]
        public async Task<ActionResult<ServiceResponse<IEnumerable<TempAndHum>>>> GetByRoom(string room)
        {
            var response = new ServiceResponse<IEnumerable<TempAndHum>>();
            response.Data = await _context.TempAndHums.
            Where(entries => entries.Room == room).ToListAsync();
            if (response.Data.Count() == 0)
            {
                response.Message = "There is no data in the specified room!";
                return Ok(response);
            }
            return Ok(response);
        }
        [Authorize]
        [HttpGet("GetCurrentData")]
        public async Task<ActionResult<ServiceResponse<TempAndHum>>> GetCurrentData(DateTime currenttime, string room)
        {
            var response = new ServiceResponse<TempAndHum>();
            response.Data = await _context.TempAndHums
                .Where(entries =>
                    entries.Date >= currenttime.AddMinutes(-2)
                    && entries.Date <= currenttime
                    && entries.Room == room)
                .FirstOrDefaultAsync();
            if(response.Data == null)
            {
                response.Message = "No data available from the sensor!";
                response.Success = false;
                return Ok(response);
            }
            response.Message = "Successfully get current data!";
            response.Success = true;
            return Ok(response);
        }


        [HttpGet("getbydate")]
        public async Task<ActionResult<ServiceResponse<IEnumerable<TempAndHum>>>> GetByDate(DateTime start, DateTime end, string room)
        {
            var response = new ServiceResponse<IEnumerable<TempAndHum>>();
            //var cacheId = _redisCacheService.Get<IEnumerable<TempAndHum>>($"{start}:{end}:{room}");

            // if(cacheId != null)
            // {
            //     Console.WriteLine("HIT THE CACHE");
            //     return cacheId;
            // }
            // else
            // {
            response.Data =
            await _context.TempAndHums.Where(entries =>
            start <= entries.Date
            && entries.Date <= end && entries.Room == room).ToListAsync();
            //_redisCacheService.Set($"{start}:{end}:{room}", tempAndHumByDates);
            if (response.Data.Count() == 0)
            {
                response.Message = "There is no data for this period in the specified room!";
                return Ok(response);
            }
            return Ok(response);
            // }

        }
    }

}