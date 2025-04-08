using ReleaseManagerIdentityApi.Data;
using ReleaseManagerIdentityApi.Models.DTOs;
using ReleaseManagerIdentityApi.Models.DTOs.Requests;
using System.Security.Cryptography;
using System.Text;

namespace ReleaseManagerIdentityApi.Services.UserService
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UserProfileDto> GetUserProfileAsync(Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }

            return new UserProfileDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                AvatarUrl = user.AvatarUrl,
                TimeZone = user.TimeZone,
                LastLoggedInOn = user.LastLoggedInOn,
                CreatedOn = user.CreatedOn
            };
        }

        public async Task UpdateUserProfileAsync(Guid userId, UpdateProfileRequest request)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }

            // Update user properties
            if (!string.IsNullOrEmpty(request.FirstName))
                user.FirstName = request.FirstName;

            if (!string.IsNullOrEmpty(request.LastName))
                user.LastName = request.LastName;

            if (!string.IsNullOrEmpty(request.TimeZone))
                user.TimeZone = request.TimeZone;

            if (!string.IsNullOrEmpty(request.AvatarUrl))
                user.AvatarUrl = request.AvatarUrl;

            user.UpdatedOn = DateTime.UtcNow;
            user.UpdatedBy = userId;

            await _context.SaveChangesAsync();
        }

        public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }

            // Verify current password
            var hashedCurrentPassword = HashPassword(request.CurrentPassword, user.PasswordSalt);
            if (hashedCurrentPassword != user.Password)
            {
                throw new InvalidOperationException("Current password is incorrect");
            }

            // Verify new password and confirmation match
            if (request.NewPassword != request.ConfirmNewPassword)
            {
                throw new InvalidOperationException("New password and confirmation do not match");
            }

            // Update password
            user.Password = HashPassword(request.NewPassword, user.PasswordSalt);
            user.UpdatedOn = DateTime.UtcNow;
            user.UpdatedBy = userId;

            await _context.SaveChangesAsync();
        }

        private string HashPassword(string password, string salt)
        {
            using var sha256 = SHA256.Create();
            var passwordWithSalt = Encoding.UTF8.GetBytes(password + salt);
            var hashBytes = sha256.ComputeHash(passwordWithSalt);
            return Convert.ToBase64String(hashBytes);
        }
    }
}