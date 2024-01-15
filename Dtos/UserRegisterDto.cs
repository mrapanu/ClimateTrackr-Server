namespace ClimateTrackr_Server.Dtos
{
    public class UserRegisterDto
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public UserType UserType {get; set;} = UserType.Normal;
    }
}