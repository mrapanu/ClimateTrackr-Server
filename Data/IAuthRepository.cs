namespace ClimateTrackr_Server.Data
{
    public interface IAuthRepository
    {
        Task<ServiceResponse<int>> Register(User user, string password);
        Task<ServiceResponse<string>> Login(string username, string password);
        Task<bool> UserExists(string username);
        Task<ServiceResponse<string>> ResetPassword(string username, string newpassword, string oldpassword);
        Task<ServiceResponse<int>> DeleteUser(string username);
    }
}