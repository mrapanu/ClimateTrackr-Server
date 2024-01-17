namespace ClimateTrackr_Server.Dtos
{
    public class GetUserDto
    {
        public string Username { get; set; } = string.Empty;
        public UserType Role { get; set; }  
        public int Id { get; set; }
    }
}