namespace ClimateTrackr_Server.Dtos
{
    public class GetProfileDto
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public bool EnableNotifications { get; set; }
        public List<UserRoom> SelectedRoomNames { get; set; } = new List<UserRoom>();
        public NotificationFrequency Frequency { get; set; } = NotificationFrequency.None;
    }
}