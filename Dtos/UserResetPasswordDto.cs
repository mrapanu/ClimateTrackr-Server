using System.ComponentModel.DataAnnotations;

namespace ClimateTrackr_Server.Dtos
{
    public class UserResetPasswordDto
    {
        public string Username { get; set; } = string.Empty;
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        public string NewPassword { get; set; } = string.Empty;
        public string OldPassword { get; set; } = string.Empty;
    }
}