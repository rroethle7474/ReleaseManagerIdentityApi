using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ReleaseManagerIdentityApi.Data;
using ReleaseManagerIdentityApi.Models.Entities;
using ReleaseManagerIdentityApi.Utilities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ReleaseManagerIdentityApi.Services.Auth
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;

        public TokenService(IConfiguration configuration, ApplicationDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        public string GenerateAccessToken(User user, Guid organizationId, IEnumerable<string> roles = null)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("organizationId", organizationId.ToString()),
                new Claim("firstName", user.FirstName),
                new Claim("lastName", user.LastName)
            };

            // Add roles to claims if available
            if (roles != null)
            {
                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpiryInMinutes"])),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<UserToken> CreateUserRefreshTokenAsync(User user)
        {
            // Get the refresh token type ID
            var refreshTokenType = await _context.TokenTypes
                .FirstOrDefaultAsync(t => t.Name == "RefreshToken");

            if (refreshTokenType == null)
            {
                throw new InvalidOperationException("RefreshToken type not found in database");
            }

            var refreshToken = new UserToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenTypeId = refreshTokenType.Id,
                TokenValue = SecurityUtilities.GenerateRefreshToken(),
                ExpiresOn = DateTime.UtcNow.AddDays(7),
                CreatedOn = DateTime.UtcNow,
                CreatedBy = user.Id,
                UpdatedOn = DateTime.UtcNow,
                UpdatedBy = user.Id
            };

            _context.UserTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            return refreshToken;
        }

        public async Task<UserToken> GetRefreshTokenAsync(string token)
        {
            return await _context.UserTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.TokenValue == token && t.ExpiresOn > DateTime.UtcNow);
        }

        public async Task RevokeRefreshTokenAsync(string token)
        {
            var refreshToken = await _context.UserTokens.FirstOrDefaultAsync(t => t.TokenValue == token);
            if (refreshToken != null)
            {
                _context.UserTokens.Remove(refreshToken);
                await _context.SaveChangesAsync();
            }
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }

            return principal;
        }
    }
}