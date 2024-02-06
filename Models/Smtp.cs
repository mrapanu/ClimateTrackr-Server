namespace ClimateTrackr_Server.Models
{
    public class Smtp
    {
        public int Id { get; set; }
        public string SmtpServer { get; set; } = string.Empty;
        public int SmtpPort { get; set; } = 25;
        public string SmtpEmail { get; set; } = string.Empty;
        public string SmtpHelo { get; set; } = string.Empty;
        public ConnectionSecurity ConnSecurity { get; set; } = ConnectionSecurity.None;
        public AuthenticationOption AuthOption { get; set; } = AuthenticationOption.None;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public enum ConnectionSecurity
    {
        None = 0,
        STARTTLS = 1,
        SSLTLS = 2
    }

    public enum AuthenticationOption
    {
        None = 0,
        UserAndPassword = 1,
    }
}