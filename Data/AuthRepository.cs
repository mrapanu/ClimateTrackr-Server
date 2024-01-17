
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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

        public async Task<ServiceResponse<int>> Register(User user, string password)
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
                Expires = DateTime.Now.AddDays(1),
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
    }
}