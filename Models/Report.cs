namespace ClimateTrackr_Server.Models
{
    public class Report
    {
        public int Id { get; set; }
        public string RoomName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public byte[] PdfContent { get; set; } = new byte[0];
        public ReportType Type { get; set; } = ReportType.Daily;
    }

    public enum ReportType
    {
        Daily = 0,
        Weekly = 1,
        Monthly = 2
    }
}