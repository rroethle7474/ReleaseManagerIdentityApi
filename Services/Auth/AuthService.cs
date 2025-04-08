using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ReleaseManagerIdentityApi.Data;
using ReleaseManagerIdentityApi.Models.DTOs;
using ReleaseManagerIdentityApi.Models.DTOs.Requests;
using ReleaseManagerIdentityApi.Models.DTOs.Responses;
using ReleaseManagerIdentityApi.Models.Entities;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ReleaseManagerIdentityApi.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;

        public AuthService(
            ApplicationDbContext context,
            ITokenService tokenService,
            IConfiguration configuration)
        {
            _context = context;
            _tokenService = tokenService;
            _configuration = configuration;
        }

        public async Task<AuthResponse> RegisterUserAsync(RegisterRequest request)
        {
            // Check if user already exists
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                throw new InvalidOperationException("User with this email already exists");
            }

            // Create salt and hash password
            var salt = GenerateSalt();
            var hashedPassword = HashPassword(request.Password, salt);

            // Create user
            var userId = Guid.NewGuid();
            var user = new User
            {
                Id = userId,
                Email = request.Email,
                Password = hashedPassword,
                PasswordSalt = salt,
                FirstName = request.FirstName,
                LastName = request.LastName,
                TimeZone = request.TimeZone,
                IsActivated = true,
                IsLocked = false,
                IsInvite = false,
                FailedPasswordAttempts = 0,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = userId, // Self-reference for creation
                UpdatedOn = DateTime.UtcNow,
                UpdatedBy = userId
            };

            // Create organization
            var organizationId = Guid.NewGuid();
            var organization = new Organization
            {
                Id = organizationId,
                Name = request.OrganizationName,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = userId,
                UpdatedOn = DateTime.UtcNow,
                UpdatedBy = userId
            };

            // Create organization user relationship
            var organizationUser = new OrganizationUser
            {
                Id = Guid.NewGuid(),
                OrganizationId = organizationId,
                UserId = userId,
                IsAdmin = true,
                IsBilling = true,
                CanInvite = true,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = userId,
                UpdatedOn = DateTime.UtcNow,
                UpdatedBy = userId
            };

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                _context.Users.Add(user);
                _context.Organizations.Add(organization);
                await _context.SaveChangesAsync();

                _context.OrganizationUsers.Add(organizationUser);
                await _context.SaveChangesAsync();

                // Generate refresh token
                var refreshToken = await _tokenService.CreateUserRefreshTokenAsync(user);

                // Generate access token
                var accessToken = _tokenService.GenerateAccessToken(user, organizationId);

                await transaction.CommitAsync();

                return new AuthResponse
                {
                    UserId = user.Id,
                    OrganizationId = organizationId,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Token = accessToken,
                    RefreshToken = refreshToken.TokenValue,
                    Expiration = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpiryInMinutes"]))
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            // Find user by email
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
            {
                throw new InvalidOperationException("Invalid email or password");
            }

            // Check if account is locked
            if (user.IsLocked && user.LockoutExpiresOn > DateTime.UtcNow)
            {
                throw new InvalidOperationException("Account is locked. Try again later.");
            }

            // Verify password
            var hashedPassword = HashPassword(request.Password, user.PasswordSalt);
            if (hashedPassword != user.Password)
            {
                // Increment failed attempts
                user.FailedPasswordAttempts++;

                // Lock account if too many failed attempts
                if (user.FailedPasswordAttempts >= 5)
                {
                    user.IsLocked = true;
                    user.LockoutExpiresOn = DateTime.UtcNow.AddMinutes(15);
                }

                await _context.SaveChangesAsync();
                throw new InvalidOperationException("Invalid email or password");
            }

            // Reset failed attempts on successful login
            user.FailedPasswordAttempts = 0;
            user.IsLocked = false;
            user.LastLoggedInOn = DateTime.UtcNow;
            user.LastActivityOn = DateTime.UtcNow;

            // Get user's organization
            var orgUser = await _context.OrganizationUsers
                .FirstOrDefaultAsync(ou => ou.UserId == user.Id);

            if (orgUser == null)
            {
                throw new InvalidOperationException("User is not associated with any organization");
            }

            // Generate refresh token
            var refreshToken = await _tokenService.CreateUserRefreshTokenAsync(user);

            // Generate access token
            var accessToken = _tokenService.GenerateAccessToken(user, orgUser.OrganizationId);

            await _context.SaveChangesAsync();

            return new AuthResponse
            {
                UserId = user.Id,
                OrganizationId = orgUser.OrganizationId,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Token = accessToken,
                RefreshToken = refreshToken.TokenValue,
                Expiration = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpiryInMinutes"]))
            };
        }

        public async Task<AuthResponse> RefreshTokenAsync(string accessToken, string refreshToken)
        {
            var principal = _tokenService.GetPrincipalFromExpiredToken(accessToken);
            var userId = Guid.Parse(principal.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var storedRefreshToken = await _tokenService.GetRefreshTokenAsync(refreshToken);

            if (storedRefreshToken == null || storedRefreshToken.UserId != userId)
            {
                throw new SecurityTokenException("Invalid refresh token");
            }

            var user = await _context.Users.FindAsync(userId);

            // Get user's organization
            var orgUser = await _context.OrganizationUsers
                .FirstOrDefaultAsync(ou => ou.UserId == user.Id);

            if (orgUser == null)
            {
                throw new InvalidOperationException("User is not associated with any organization");
            }

            // Revoke the current refresh token
            await _tokenService.RevokeRefreshTokenAsync(refreshToken);

            // Generate new refresh token
            var newRefreshToken = await _tokenService.CreateUserRefreshTokenAsync(user);

            // Generate new access token
            var newAccessToken = _tokenService.GenerateAccessToken(user, orgUser.OrganizationId);

            return new AuthResponse
            {
                UserId = user.Id,
                OrganizationId = orgUser.OrganizationId,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Token = newAccessToken,
                RefreshToken = newRefreshToken.TokenValue,
                Expiration = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpiryInMinutes"]))
            };
        }

        public async Task RevokeTokenAsync(string refreshToken)
        {
            await _tokenService.RevokeRefreshTokenAsync(refreshToken);
        }

        private string GenerateSalt()
        {
            var randomBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
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