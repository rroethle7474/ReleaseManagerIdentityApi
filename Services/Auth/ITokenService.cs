using ReleaseManagerIdentityApi.Models.Entities;
using System.Security.Claims;

namespace ReleaseManagerIdentityApi.Services.Auth
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user, Guid organizationId, IEnumerable<string> roles = null);
        Task<UserToken> CreateUserRefreshTokenAsync(User user);
        Task<UserToken> GetRefreshTokenAsync(string token);
        Task RevokeRefreshTokenAsync(string token);
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}