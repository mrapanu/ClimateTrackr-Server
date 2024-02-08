using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ClimateTrackr_Server.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace ClimateTrackr_Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _authRepo;
        private readonly DataContext _context;

        public AuthController(IAuthRepository authRepo, DataContext context)
        {
            _authRepo = authRepo;
            _context = context;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("AddUser")]
        public async Task<ActionResult<ServiceResponse<int>>> AddUser(AddUserDto request)
        {
            var response = await _authRepo.AddUser(new Models.User { Username = request.Username, Usertype = request.UserType }, request.Password);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            History hist = new History
            {
                DateTime = DateTime.Now,
                User = User.FindFirst(ClaimTypes.Name)!.Value,
                ActionMessage = $"Added new user '{request.Username}'."
            };
            _context.History.Add(hist);
            await _context.SaveChangesAsync();
            return Ok(response);
        }

        [HttpPost("Login")]
        public async Task<ActionResult<ServiceResponse<int>>> Login(UserLoginDto request)
        {
            var response = await _authRepo.Login(request.Username, request.Password);
            if (!response.Success)
            {
                return Unauthorized(response);
            }
            History hist = new History
            {
                DateTime = DateTime.Now,
                User = request.Username,
                ActionMessage = "Logged in successfully!"
            };
            _context.History.Add(hist);
            await _context.SaveChangesAsync();
            return Ok(response);
        }

        [Authorize]
        [HttpPost("ResetPassword")]
        public async Task<ActionResult<ServiceResponse<string>>> ResetPassword(UserResetPasswordDto request)
        {

            var response = await _authRepo.ResetPassword(request.Username, request.NewPassword, request.OldPassword);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            History hist = new History
            {
                DateTime = DateTime.Now,
                User = request.Username,
                ActionMessage = "Reset password successfully!"
            };
            _context.History.Add(hist);
            await _context.SaveChangesAsync();
            return Ok(response);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("ChangePassword")]
        public async Task<ActionResult<ServiceResponse<string>>> ChangePassword(ChangePasswordDto request)
        {

            var response = await _authRepo.ChangePassword(request.Username, request.NewPassword);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            History hist = new History
            {
                DateTime = DateTime.Now,
                User = User.FindFirst(ClaimTypes.Name)!.Value,
                ActionMessage = $"Changed password for '{request.Username}'."
            };
            _context.History.Add(hist);
            await _context.SaveChangesAsync();
            return Ok(response);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("ChangeRole")]
        public async Task<ActionResult<ServiceResponse<string>>> ChangeRole(ChangeRoleDto request)
        {

            var response = await _authRepo.ChangeRole(request.Username, request.Role);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            History hist = new History
            {
                DateTime = DateTime.Now,
                User = User.FindFirst(ClaimTypes.Name)!.Value,
                ActionMessage = $"Changed the role to '{request.Role.ToString()}' for '{request.Username}'."
            };
            _context.History.Add(hist);
            await _context.SaveChangesAsync();
            return Ok(response);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("DeleteUser")]
        public async Task<ActionResult<ServiceResponse<int>>> DeleteUser(string username)
        {

            var response = await _authRepo.DeleteUser(username);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            History hist = new History
            {
                DateTime = DateTime.Now,
                User = User.FindFirst(ClaimTypes.Name)!.Value,
                ActionMessage = $"Deleted user '{username}'."
            };
            _context.History.Add(hist);
            await _context.SaveChangesAsync();
            return Ok(response);
        }

        [Authorize]
        [HttpPut("UpdateProfile")]
        public async Task<ActionResult<ServiceResponse<string>>> UpdateProfile(UpdateUserProfileDto request)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            var response = await _authRepo.UpdateProfile(username!, request.Email, request.FullName);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            History hist = new History
            {
                DateTime = DateTime.Now,
                User = username!,
                ActionMessage = "Updated the profile successfully!"
            };
            _context.History.Add(hist);
            await _context.SaveChangesAsync();
            return Ok(response);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("GetUsers")]
        public async Task<ActionResult<ServiceResponse<IEnumerable<GetUserDto>>>> GetUsers()
        {
            var response = await _authRepo.GetUsers();
            return Ok(response);
        }

        [Authorize]
        [HttpGet("GetProfileInfo")]
        public async Task<ActionResult<ServiceResponse<GetProfileDto>>> GetProfileInfo()
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            var response = await _authRepo.GetProfile(username!);
            return Ok(response);
        }

        [Authorize]
        [HttpPut("UpdateEnableNotifications")]
        public async Task<ActionResult<ServiceResponse<string>>> UpdateEnableNotifications(bool enableNotifications)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            var response = await _authRepo.SetNotifications(username!, enableNotifications);
            
            if (!response.Success)
            {
                return BadRequest(response);
            }
            History hist = new History
            {
                DateTime = DateTime.Now,
                User = User.FindFirst(ClaimTypes.Name)!.Value,
                ActionMessage = $"Set Notifications to '{enableNotifications}'!",
            };
            _context.History.Add(hist);
            await _context.SaveChangesAsync();
            return Ok(response);
        }
    }
}