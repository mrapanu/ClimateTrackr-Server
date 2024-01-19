namespace ClimateTrackr_Server.Models
{
    public class RoomConfig
    {
        public int Id { get; set; }
        public string RoomName { get; set; } = string.Empty;
        public Window  Window { get; set; } 
    }

    public enum Window
    {
        Window1 = 1,
        Window2 = 2,
        Window3 = 3,
        Window4 = 4
    }
}