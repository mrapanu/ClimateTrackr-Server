
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.RegularExpressions;
using ClimateTrackr_Server.Dtos;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.IdentityModel.Tokens;

namespace ClimateTrackr_Server.Data
{
    public class AuthRepository : IAuthRepository
    {
        private readonly DataContext _context;

        public AuthRepository(DataContext context)
        {
            _context = context;
        }
        public async Task<ServiceResponse<string>> Login(string username, string password)
        {
            var response = new ServiceResponse<string>();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username.ToLower().Equals(username.ToLower()));
            if (user is null)
            {
                response.Success = false;
                response.Message = "User / Password combination wrong.";
            }
            else if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
            {
                response.Success = false;
                response.Message = "User / Password combination wrong.";
            }
            else
            {
                response.Data = CreateToken(user);
                response.Message = "User successfully authenticated.";
            }
            return response;
        }

        public async Task<ServiceResponse<int>> AddUser(User user, string password)
        {
            var response = new ServiceResponse<int>();

            if (await UserExists(user.Username))
            {
                response.Success = false;
                response.Message = "User already exists!";
                return response;
            }
            CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            response.Data = user.Id;
            response.Message = "User created successfully.";
            return response;
        }

        public async Task<bool> UserExists(string username)
        {
            if (await _context.Users.AnyAsync(u => u.Username.ToLower() == username.ToLower()))
            {
                return true;
            }
            return false;
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }

        private string CreateToken(User user)
        {
            var claims = new List<Claim>{
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Usertype.ToString())
            };

            var appSettingsToken = Environment.GetEnvironmentVariable("JWT_SECRET_TOKEN")!;

            if (appSettingsToken is null)
            {
                throw new Exception("SecretToken is null");
            }

            SymmetricSecurityKey key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(appSettingsToken));
            SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddHours(12),
                SigningCredentials = creds
            };

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        public async Task<ServiceResponse<string>> ResetPassword(string username, string newpassword, string oldpassword)
        {
            var response = new ServiceResponse<string>();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username.ToLower().Equals(username.ToLower()));
            if (user is null)
            {
                response.Success = false;
                response.Message = $"Can't reset the password for {username}";
            }
            else if (!VerifyPasswordHash(oldpassword, user.PasswordHash, user.PasswordSalt))
            {
                response.Success = false;
                response.Message = "The old password is wrong!";
            }
            else
            {
                CreatePasswordHash(newpassword, out byte[] passwordHash, out byte[] passwordSalt);
                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;
                await _context.SaveChangesAsync();
                response.Data = CreateToken(user);
                response.Message = "Password reset successfully!";
            }
            return response;
        }

        public async Task<ServiceResponse<int>> DeleteUser(string username)
        {
            var response = new ServiceResponse<int>();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username.ToLower().Equals(username.ToLower()));
            if (user is null)
            {
                response.Success = false;
                response.Message = $"Can't delete the {username} user.";
            }
            else
            {
                var nsUpdate = await _context.NotificationSettings.FirstOrDefaultAsync(ns => ns.UserId == user.Id);
                var userRoomsToRemove = _context.NotificationSettings.
                Where(ns => ns.UserId == user.Id).SelectMany(userroom => userroom.SelectedRoomNames);
                _context.UserRooms.RemoveRange(userRoomsToRemove);
                _context.NotificationSettings.Remove(nsUpdate!);
                await _context.SaveChangesAsync();
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                response.Data = user.Id;
                response.Message = $"User {username} has been deleted.";
            }
            return response;
        }

        public async Task<ServiceResponse<List<GetUserDto>>> GetUsers()
        {
            var response = new ServiceResponse<List<GetUserDto>>();

            var users = await _context.Users.ToListAsync();
            var userDtos = new List<GetUserDto>();

            if (users.Count() != 0)
            {
                foreach (var user in users)
                {
                    userDtos.Add(new GetUserDto
                    {
                        Id = user.Id,
                        Username = user.Username,
                        Role = user.Usertype
                    });
                }
                response.Data = userDtos;
                response.Success = true;
                response.Message = "Get users was successfully executed.";
            }
            else
            {
                response.Success = false;
                response.Message = "There is no user in the table.";
            }

            return response;

        }

        public async Task<ServiceResponse<string>> ChangePassword(string username, string password)
        {
            var response = new ServiceResponse<string>();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username.ToLower().Equals(username.ToLower()));
            if (user is null)
            {
                response.Success = false;
                response.Message = $"Can't change the password for {username}";
            }
            else
            {
                CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);
                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;
                await _context.SaveChangesAsync();
                response.Data = user.Username;
                response.Message = "Password changed successfully!";
            }
            return response;
        }

        public async Task<ServiceResponse<string>> ChangeRole(string username, UserType role)
        {
            var response = new ServiceResponse<string>();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username.ToLower().Equals(username.ToLower()));
            if (user is null)
            {
                response.Success = false;
                response.Message = $"Can't change the role for {username}";
            }
            else
            {
                user.Usertype = role;
                await _context.SaveChangesAsync();
                response.Data = user.Username;
                response.Message = "Role changed successfully!";
            }
            return response;
        }

        public async Task<ServiceResponse<UpdateUserProfileDto>> UpdateProfile(string username, string email, string fullName)
        {
            var response = new ServiceResponse<UpdateUserProfileDto>();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username.ToLower().Equals(username.ToLower()));
            if (user is null)
            {
                response.Success = false;
                response.Message = "Can't update the profile!";
            }
            else
            {
                if (!email.IsNullOrEmpty())
                {
                    user.Email = email;
                }
                if (!fullName.IsNullOrEmpty())
                {
                    user.FullName = fullName;
                }
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                var responseData = new UpdateUserProfileDto { Username = user.Username, FullName = user.FullName, Email = user.Email };
                response.Data = responseData;
                response.Message = "Profile updated successfully!";
            }
            return response;
        }

        public async Task<ServiceResponse<GetProfileDto>> SetNotifications(string username, bool setNotifications)
        {
            var response = new ServiceResponse<GetProfileDto>();
            string emailPattern = @"^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username.ToLower().Equals(username.ToLower()));
            if (setNotifications == true && !Regex.IsMatch(user!.Email, emailPattern))
            {
                response.Message = "You must set a valid email address for your user!";
                response.Success = false;
                return response;
            }
            if (user is null)
            {
                response.Success = false;
                response.Message = "Can't update the notifications!";
            }

            else
            {
                user.EnableNotifications = setNotifications;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                var responseData = new GetProfileDto { Username = user.Username, FullName = user.FullName, Email = user.Email, EnableNotifications = user.EnableNotifications };
                response.Data = responseData;
                response.Message = "Notifications updated successfully!";
            }
            return response;
        }

        public async Task<ServiceResponse<GetProfileDto>> GetProfile(string username)
        {
            var response = new ServiceResponse<GetProfileDto>();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username.ToLower().Equals(username.ToLower()));
            if (user is null)
            {
                response.Success = false;
                response.Message = "User not exist!";
            }
            else
            {
                var responseData = new GetProfileDto { FullName = user.FullName, Email = user.Email, Username = user.Username, EnableNotifications = user.EnableNotifications };
                response.Data = responseData;
                response.Message = "Successfully received data for user.";
            }
            return response;
        }
    }
}