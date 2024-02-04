using ClimateTrackr_Server.Dtos;

namespace ClimateTrackr_Server.Data
{
    public interface IAuthRepository
    {
        Task<ServiceResponse<int>> AddUser(User user, string password);
        Task<ServiceResponse<string>> Login(string username, string password);
        Task<bool> UserExists(string username);
        Task<ServiceResponse<string>> ResetPassword(string username, string newpassword, string oldpassword);
        Task<ServiceResponse<string>> ChangePassword(string username, string password);
        Task<ServiceResponse<string>> ChangeRole(string username, UserType role);
        Task<ServiceResponse<GetProfileDto>> UpdateProfile(string username, string email, string fullName);
        Task<ServiceResponse<GetProfileDto>> GetProfile(string username);
        Task<ServiceResponse<int>> DeleteUser(string username);
        Task<ServiceResponse<List<GetUserDto>>> GetUsers();
        Task<ServiceResponse<GetProfileDto>> SetNotifications(string username, bool setNotifications);
    }
}