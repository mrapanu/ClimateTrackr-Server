using System.Data;
using System.Security.Claims;
using System.Text.RegularExpressions;
using ClimateTrackr_Server.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClimateTrackr_Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationSettingsController : ControllerBase
    {
        private readonly DataContext _context;
        public NotificationSettingsController(DataContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpPost("AddNotificationSettings")]
        public async Task<ActionResult<ServiceResponse<GetProfileDto>>> AddNotificationSettings(AddNotificationSettingsDto request)
        {
            string emailPattern = @"^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            var response = new ServiceResponse<GetProfileDto>();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId);
            if (!Regex.IsMatch(request.UserEmail, emailPattern))
            {
                response.Message = "You must set a valid email address for your user!";
                response.Success = false;
                return BadRequest(response);
            }

            if (!Enum.IsDefined(typeof(NotificationFrequency), request.Frequency))
            {
                response.Message = "Selected Notification Frequency doesn't exist.";
                response.Success = false;
                return BadRequest(response);
            }

            if (await NotificationSettingsExist(request.UserId))
            {
                var nsUpdate = await _context.NotificationSettings.FirstOrDefaultAsync(ns => ns.UserId == request.UserId);
                nsUpdate!.Frequency = request.Frequency;
                nsUpdate!.UserEmail = request.UserEmail;
                nsUpdate.SelectedRoomNames = request.RoomNames;
                var userRoomsToRemove = _context.NotificationSettings.
                Where(ns => ns.UserId == request.UserId).SelectMany(userroom => userroom.SelectedRoomNames);
                _context.UserRooms.RemoveRange(userRoomsToRemove);
                _context.NotificationSettings.Update(nsUpdate);
                await _context.SaveChangesAsync();
                var userRooms = _context.NotificationSettings.Where(ns => ns.UserId == user!.Id)
                .SelectMany(userroom => userroom.SelectedRoomNames);
                var notifSettings = await _context.NotificationSettings.FirstOrDefaultAsync(ns => ns.UserId == user!.Id);
                response.Success = true;
                response.Message = "Notification Settings Updated!";
                var responseData = new GetProfileDto
                {
                    UserId = user!.Id,
                    Username = user.Username,
                    FullName = user.FullName,
                    Email = user.Email,
                    EnableNotifications = user.EnableNotifications,
                    SelectedRoomNames = userRooms.ToList(),
                    Frequency = notifSettings!.Frequency,
                };
                response.Data = responseData;
                History hist = new History
                {
                    DateTime = DateTime.Now,
                    User = User.FindFirst(ClaimTypes.Name)!.Value,
                    ActionMessage = "Updated Notification Settings successfully!",
                };
                _context.History.Add(hist);
                await _context.SaveChangesAsync();
                return Ok(response);
            }
            else
            {
                var notificationSettings = new NotificationSettings
                {
                    Frequency = request.Frequency,
                    UserEmail = request.UserEmail,
                    UserId = request.UserId,
                    SelectedRoomNames = request.RoomNames,
                };
                _context.NotificationSettings.Add(notificationSettings);
                await _context.SaveChangesAsync();
                var userRooms = _context.NotificationSettings.Where(ns => ns.UserId == user!.Id)
                .SelectMany(userroom => userroom.SelectedRoomNames);
                var notifSettings = await _context.NotificationSettings.FirstOrDefaultAsync(ns => ns.UserId == user!.Id);
                response.Success = true;
                response.Message = "Successfully set notification settings.";
                var responseData = new GetProfileDto
                {
                    UserId = user!.Id,
                    Username = user.Username,
                    FullName = user.FullName,
                    Email = user.Email,
                    EnableNotifications = user.EnableNotifications,
                    SelectedRoomNames = userRooms.ToList(),
                    Frequency = notifSettings!.Frequency,
                };
                response.Data = responseData;
                History hist = new History
                {
                    DateTime = DateTime.Now,
                    User = User.FindFirst(ClaimTypes.Name)!.Value,
                    ActionMessage = "Created Notification Settings successfully!",
                };
                _context.History.Add(hist);
                await _context.SaveChangesAsync();
                return Ok(response);
            }

        }

        private async Task<bool> NotificationSettingsExist(int id)
        {
            if (await _context.NotificationSettings.AnyAsync(ns => ns.UserId == id))
            {
                return true;
            }
            return false;
        }
    }
}