using ReleaseManagerIdentityApi.Models.DTOs.Responses;

namespace ReleaseManagerIdentityApi.Services.Auth
{
    public interface IEntraTokenExchangeService
    {
        Task<TokenResponse> ExchangeCodeForTokenAsync(string code, string clientId, string clientSecret, string redirectUri, string tenantId);
        Task<CloudProviderTokenResponse> RefreshEntraTokenAsync(Guid userId, int providerId);
    }
}