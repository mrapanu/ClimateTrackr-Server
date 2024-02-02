using System.ComponentModel.DataAnnotations.Schema;

namespace ClimateTrackr_Server.Models
{
    public class NotificationSettings
    {
        public int Id { get; set; }
        public string UserEmail { get; set; } = string.Empty;
        public List<UserRoom> SelectedRoomNames { get; set; } = new List<UserRoom>();
        public NotificationFrequency Frequency { get; set; } = NotificationFrequency.None;
        public int UserId { get; set; }
        public User? User { get; set; }
    }

    public enum NotificationFrequency
    {
        None = 0,
        Daily = 1,
        Weekly = 2,
        DailyWeekly = 3,
        Monthly = 4,
        DailyMonthly = 5,
        WeeklyMonthly = 6,
        All = 7,

    }
}