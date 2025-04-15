using Microsoft.EntityFrameworkCore;
using ReleaseManagerIdentityApi.Data;
using ReleaseManagerIdentityApi.Models.DTOs.Responses;
using ReleaseManagerIdentityApi.Models.Entities;
using ReleaseManagerIdentityApi.Services.Clients;
using ReleaseManagerIdentityApi.Services.DevOpsServices;

namespace ReleaseManagerIdentityApi.Services.Auth
{
    public class EntraTokenExchangeService : IEntraTokenExchangeService
    {
        private readonly ICloudProviderService _cloudProviderService;
        private readonly IEntraTokenExchangeApiClient _entraTokenApiClient;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;

        public EntraTokenExchangeService(ICloudProviderService cloudProviderService, IEntraTokenExchangeApiClient entraTokenApiClient, IConfiguration configuration, ApplicationDbContext context)
        {
            _cloudProviderService = cloudProviderService;
            _entraTokenApiClient = entraTokenApiClient;
            _configuration = configuration;
            _context = context;
        }

        public async Task<TokenResponse> ExchangeCodeForTokenAsync(
            string code, string clientId, string clientSecret, string redirectUri, string tenantId)
        {
            return await _entraTokenApiClient.ExchangeCodeForTokenAsync(code, clientId, clientSecret, redirectUri, tenantId);
        }

        public async Task<CloudProviderTokenResponse> RefreshEntraTokenAsync(Guid userId, int providerId)
        {
            // Get the refresh token from storage
            var tokenType = await _context.TokenTypes.FirstOrDefaultAsync(t => t.Name == "EntraOAuthRefreshTokenn");
            if (tokenType == null)
            {
                throw new InvalidOperationException("EntraOAuthRefreshToken type not found");
            }

            var refreshToken = await _context.UserTokens
                .Where(t => t.UserId == userId &&
                            t.TokenTypeId == tokenType.Id)
                .FirstOrDefaultAsync();

            if (refreshToken == null || string.IsNullOrEmpty(refreshToken.TokenValue))
            {
                throw new InvalidOperationException("Refresh token not found");
            }

            // Exchange for new token
            var clientId = _configuration["MicrosoftEntra:ClientId"];
            var clientSecret = _configuration["MicrosoftEntra:ClientSecret"];
            var tenantId = _configuration["MicrosoftEntra:TenantId"];

            var tokenResponse = await _entraTokenApiClient.RefreshTokenAsync(refreshToken.TokenValue, clientId, clientSecret, tenantId);

            // Store the new tokens
            await StoreEntraTokensAsync(userId, providerId, tokenResponse);

            return new CloudProviderTokenResponse { Token = tokenResponse.AccessToken };
        }

        private async Task StoreEntraTokensAsync(Guid userId, int providerId, TokenResponse tokenResponse)
        {
            // Store access token
            await _cloudProviderService.StoreTokenAsync(
                userId,
                providerId,
                tokenResponse.AccessToken,
                DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn),
                2, // OAuth auth method
                true); // Is Entra token

            // Store refresh token if provided
            if (!string.IsNullOrEmpty(tokenResponse.RefreshToken))
            {
                var refreshTokenType = await _context.TokenTypes
                    .FirstOrDefaultAsync(t => t.Name == "EntraOAuthRefreshToken");

                if (refreshTokenType == null)
                {
                    throw new InvalidOperationException("EntraOAuthRefreshToken type not found");
                }

                var existingRefreshToken = await _context.UserTokens
                    .FirstOrDefaultAsync(t => t.UserId == userId && t.TokenTypeId == refreshTokenType.Id);

                if (existingRefreshToken != null)
                {
                    existingRefreshToken.TokenValue = tokenResponse.RefreshToken;
                    existingRefreshToken.UpdatedOn = DateTime.UtcNow;
                    existingRefreshToken.UpdatedBy = userId;
                }
                else
                {
                    var newRefreshToken = new UserToken
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        TokenTypeId = refreshTokenType.Id,
                        TokenValue = tokenResponse.RefreshToken,
                        ExpiresOn = DateTime.UtcNow.AddDays(90), // Refresh tokens typically last longer
                        CreatedOn = DateTime.UtcNow,
                        CreatedBy = userId,
                        UpdatedOn = DateTime.UtcNow,
                        UpdatedBy = userId
                    };

                    _context.UserTokens.Add(newRefreshToken);
                }

                await _context.SaveChangesAsync();
            }
        }
    }
}