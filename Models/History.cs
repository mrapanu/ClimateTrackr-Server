namespace ClimateTrackr_Server.Models
{
    public class History
    {
        public int Id { get; set; }
        public string ActionMessage { get; set; } = string.Empty;
        public DateTime DateTime { get; set; }
        public string User { get; set; } = string.Empty;
    }
}