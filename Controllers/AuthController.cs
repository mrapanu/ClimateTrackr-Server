using System.Security.Claims;
using ClimateTrackr_Server.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClimateTrackr_Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _authRepo;

        public AuthController(IAuthRepository authRepo)
        {
            _authRepo = authRepo;
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
    }
}