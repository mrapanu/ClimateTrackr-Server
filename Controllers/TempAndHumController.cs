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

        [HttpGet("GetByRoom")]
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
                    entries.Date >= currenttime.AddMinutes(-4)
                    && entries.Date <= currenttime
                    && entries.Room == room)
                .FirstOrDefaultAsync();
            if (response.Data == null)
            {
                response.Message = "No data available from the sensor!";
                response.Success = false;
                return Ok(response);
            }
            response.Message = "Successfully get current data!";
            response.Success = true;
            return Ok(response);
        }


        [HttpGet("GetByDate")]
        public async Task<ActionResult<ServiceResponse<IEnumerable<TempAndHum>>>> GetByDate(DateTime timeStart, DateTime timeEnd, string room)
        {
            var response = new ServiceResponse<IEnumerable<TempAndHum>>();
            var timeDifference = timeEnd - timeStart;
            var step = CalculateStep(timeDifference);
            response.Data = await GetTempAndHumDataWithStep(timeStart, timeEnd, room, step);

            if (response.Data.Count() == 0)
            {
                response.Message = "There is no data for this period in the specified room!";
                return Ok(response);
            }
            return Ok(response);
        }

        private async Task<IEnumerable<TempAndHum>> GetTempAndHumDataWithStep(DateTime timeStart, DateTime timeEnd, string room, TimeSpan step)
        {
            var filteredData = await _context.TempAndHums
                .Where(entry => entry.Room == room && entry.Date >= timeStart && entry.Date <= timeEnd)
                .OrderBy(entry => entry.Date)
                .ToListAsync();

            var dataWithRowNumber = filteredData
                .Select((entry, index) => new { Entry = entry, RowNumber = index + 1 })
                .ToList();

            var data = dataWithRowNumber
                .Where(x => x.RowNumber % (int)(step.TotalMinutes / 2) == 0)
                .Select(x => x.Entry);

            return data;
        }


        private TimeSpan CalculateStep(TimeSpan timeDifference)
        {
            var stepSizes = new Dictionary<TimeSpan, TimeSpan>
            {
                { TimeSpan.FromDays(1), TimeSpan.FromMinutes(2) },
                { TimeSpan.FromDays(3), TimeSpan.FromMinutes(15) },
                { TimeSpan.FromDays(7), TimeSpan.FromMinutes(30) },
                { TimeSpan.FromDays(31), TimeSpan.FromHours(1) },
                { TimeSpan.FromDays(90), TimeSpan.FromHours(2) },
                { TimeSpan.MaxValue, TimeSpan.FromHours(6) }
            };
            foreach (var kvp in stepSizes)
            {
                if (timeDifference < kvp.Key)
                    return kvp.Value;
            }
            return TimeSpan.FromHours(6);
        }
    }

}