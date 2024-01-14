namespace ClimateTrackr_Server.Models
{
    public class TempAndHum
    {
        public int Id {get; set;}
        public string Room { get; set; } = "Bedroom";
        public double Temperature { get; set; }
        public double Humidity { get; set; }
        public DateTime Date { get; set; }
    }
}