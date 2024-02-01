using System.ComponentModel.DataAnnotations;

namespace ClimateTrackr_Server.Dtos
{
    public class AddUserDto
    {
        public string Username { get; set; } = string.Empty;

        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        public string Password { get; set; } = string.Empty;
        public UserType UserType { get; set; } = UserType.Normal;
    }
}