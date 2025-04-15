using ReleaseManagerIdentityApi.Models.DTOs.Responses;

namespace ReleaseManagerIdentityApi.Services.Clients
{
    public interface IEntraTokenExchangeApiClient
    {
        Task<TokenResponse> ExchangeCodeForTokenAsync(string code, string clientId, string clientSecret, string redirectUri, string tenantId);
        Task<TokenResponse> RefreshTokenAsync(string refreshToken, string clientId, string clientSecret, string tenantId);
    }
}