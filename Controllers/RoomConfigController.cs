using ClimateTrackr_Server.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClimateTrackr_Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class RoomConfigController : ControllerBase
    {
        private readonly DataContext _context;

        public RoomConfigController(DataContext context)
        {
            _context = context;
        }

        [HttpGet("GetConfig")]
        public async Task<ActionResult<ServiceResponse<IEnumerable<RoomConfig>>>> GetConfig()
        {
            var response = new ServiceResponse<IEnumerable<RoomConfig>>();
            response.Data = await _context.RoomConfigs.ToListAsync();
            if (response.Data.Count() == 0)
            {
                response.Message = "There is no configuration available";
                response.Success = false;
                return Ok(response);
            }
            return Ok(response);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("GetRoomsFromWindow")]
        public async Task<ActionResult<ServiceResponse<IEnumerable<RoomConfig>>>> GetRoomsFromWindow(Window window)
        {
            var response = new ServiceResponse<IEnumerable<RoomConfig>>();
            response.Data = await _context.RoomConfigs.Where(r => r.Window == window).ToListAsync();
            if (!Enum.IsDefined(typeof(Window), window))
            {
                response.Message = "Window not exist!";
                response.Success = false;
                return response;
            }
            if (response.Data.Count() == 0)
            {
                response.Message = "There is no room configured in this window!";
                response.Success = true;
                return Ok(response);
            }
            return Ok(response);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("AddRoom")]
        public async Task<ActionResult<ServiceResponse<IEnumerable<RoomConfig>>>> AddRoom(AddRoomDto room)
        {
            var response = new ServiceResponse<IEnumerable<RoomConfig>>();

            var roomsInWindow = await _context.RoomConfigs.Where(r => r.Window == room.Window).ToListAsync();

            if (roomsInWindow.Count() == 3)
            {
                response.Message = "You already have 3 rooms in this window. This is the maximum!";
                response.Success = false;
                return Ok(response);
            }

            if (!Enum.IsDefined(typeof(Window), room.Window))
            {
                response.Message = "Window not exist!";
                response.Success = false;
                return response;
            }
            var newRoom = new RoomConfig()
            {
                Window = room.Window,
                RoomName = room.RoomName
            };
            _context.RoomConfigs.Add(newRoom);
            await _context.SaveChangesAsync();
            response.Data = await _context.RoomConfigs.Where(r => r.Window == room.Window).ToListAsync();
            response.Message = $"Successfully added new room to the {room.Window}";
            response.Success = true;
            return Ok(response);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("DeleteRoom")]
        public async Task<ActionResult<ServiceResponse<IEnumerable<RoomConfig>>>> DeleteRoom(int roomId)
        {
            var response = new ServiceResponse<IEnumerable<RoomConfig>>();

            var roomToDelete = await _context.RoomConfigs.FindAsync(roomId);

            if (roomToDelete == null)
            {
                response.Message = $"Room with ID {roomId} not found.";
                response.Success = false;
                return Ok(response);
            }

            _context.RoomConfigs.Remove(roomToDelete);
            await _context.SaveChangesAsync();

            response.Data = await _context.RoomConfigs.Where(w => w.Window == roomToDelete.Window).ToListAsync();
            response.Message = $"Successfully deleted room with ID {roomId}";
            response.Success = true;
            return Ok(response);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("RenameRoom")]
        public async Task<ActionResult<ServiceResponse<RoomConfig>>> RenameRoom(RenameRoomDto room)
        {
            var response = new ServiceResponse<RoomConfig>();

            var roomToUpdate = await _context.RoomConfigs.FindAsync(room.Id);

            if (roomToUpdate == null)
            {
                response.Message = $"Room with ID {room.Id} not found.";
                response.Success = false;
                return Ok(response);
            }

            roomToUpdate.RoomName = room.Name;

            _context.RoomConfigs.Update(roomToUpdate);
            await _context.SaveChangesAsync();

            response.Data = await _context.RoomConfigs.FindAsync(room.Id);
            response.Message = $"Successfully updated room with ID {room.Id}";
            response.Success = true;
            return Ok(response);
        }

    }
}