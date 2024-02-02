namespace ClimateTrackr_Server.Dtos
{
    public class AddNotificationSettingsDto
    {
        public string UserEmail { get; set; } = string.Empty;
        public NotificationFrequency Frequency { get; set; } = NotificationFrequency.None;
        public int UserId { get; set; }
        public List<UserRoom> RoomNames { get; set; } = new List<UserRoom>();
    }
}