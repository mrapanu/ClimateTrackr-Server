namespace ClimateTrackr_Server.Dtos
{
    public class GetProfileDto
    {
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public bool EnableNotifications { get; set; } 
    }
}