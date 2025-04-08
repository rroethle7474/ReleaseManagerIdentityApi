using ReleaseManagerIdentityApi.Models.DTOs;

namespace ReleaseManagerIdentityApi.Services.Auth
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterUserAsync(RegisterRequest request);
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<AuthResponse> RefreshTokenAsync(string accessToken, string refreshToken);
        Task RevokeTokenAsync(string refreshToken);
    }
}
