using ClimateTrackr_Server.Dtos;
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

        [HttpPost("Register")]
        public async Task<ActionResult<ServiceResponse<int>>> Register(UserRegisterDto request)
        {
            var response = await _authRepo.Register(new Models.User {Username = request.Username}, request.Password);
            if(!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }
        [HttpPost("Login")]
        public async Task<ActionResult<ServiceResponse<int>>> Login(UserLoginDto request)
        {
            var response = await _authRepo.Login(request.Username, request.Password);
            if(!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpPost("ResetPassword")]
        public async Task<ActionResult<ServiceResponse<int>>> ResetPassword(UserResetDto request)
        {

            var response = await _authRepo.ResetPassword(request.Username,request.NewPassword,request.OldPassword);
            if(!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }
    }
}