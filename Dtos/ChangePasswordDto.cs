namespace ClimateTrackr_Server.Dtos
{
    public class ChangePasswordDto
    {
        public string Username { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}