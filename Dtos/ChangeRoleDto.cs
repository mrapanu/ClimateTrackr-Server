namespace ClimateTrackr_Server.Dtos
{
    public class ChangeRoleDto
    {
        public string Username { get; set; } = string.Empty;
        public UserType Role { get; set; } = UserType.Normal;
    }
}