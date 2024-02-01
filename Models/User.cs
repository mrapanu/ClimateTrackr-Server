namespace ClimateTrackr_Server.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public byte[] PasswordHash { get; set; } = new byte[0];
        public byte[] PasswordSalt { get; set; } = new byte[0];
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public UserType Usertype { get; set; } = UserType.Normal;
    }
}