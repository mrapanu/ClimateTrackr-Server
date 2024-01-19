namespace ClimateTrackr_Server.Dtos
{
    public class AddRoomDto
    {
        public string RoomName {get; set;} = string.Empty;
        public Window Window { get; set; }
    }
}