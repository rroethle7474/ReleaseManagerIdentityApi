using Microsoft.EntityFrameworkCore;
using ReleaseManagerIdentityApi.Data;
using ReleaseManagerIdentityApi.Models.DTOs.Responses;
using ReleaseManagerIdentityApi.Models.Entities;
using ReleaseManagerIdentityApi.Services.DevOpsServices;
using System.Text.Json;

namespace ReleaseManagerIdentityApi.Services.Auth
{
    public class EntraTokenExchangeService : IEntraTokenExchangeService
    {
        private readonly AzureDevOpsService _azureDevOpsService;
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;

        public EntraTokenExchangeService(AzureDevOpsService azureDevOpsService, HttpClient httpClient, IConfiguration configuration, ApplicationDbContext context)
        {
            _azureDevOpsService = azureDevOpsService;
            _httpClient = httpClient;
            _configuration = configuration;
            _context = context;
        }

        public async Task<TokenResponse> ExchangeCodeForTokenAsync(
            string code, string clientId, string clientSecret, string redirectUri, string tenantId)
        {
            var requestContent = new FormUrlEncodedContent(new[]
            {
            new KeyValuePair<string, string>("client_id", clientId),
            new KeyValuePair<string, string>("client_secret", clientSecret),
            new KeyValuePair<string, string>("code", code),
            new KeyValuePair<string, string>("redirect_uri", redirectUri),
            new KeyValuePair<string, string>("grant_type", "authorization_code"),
            new KeyValuePair<string, string>("scope", "499b84ac-1321-427f-aa17-267ca6975798/.default")
        });

            var response = await _httpClient.PostAsync(
                $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token",
                requestContent);

            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TokenResponse>(responseContent);
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

            var requestContent = new FormUrlEncodedContent(new[]
            {
        new KeyValuePair<string, string>("client_id", clientId),
        new KeyValuePair<string, string>("client_secret", clientSecret),
        new KeyValuePair<string, string>("refresh_token", refreshToken.TokenValue),
        new KeyValuePair<string, string>("grant_type", "refresh_token"),
        new KeyValuePair<string, string>("scope", "499b84ac-1321-427f-aa17-267ca6975798/.default")
    });

            var response = await _httpClient.PostAsync(
                $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token",
                requestContent);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException("Failed to refresh token");
            }

            var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();

            // Store the new tokens
            await StoreEntraTokensAsync(userId, providerId, tokenResponse);

            return new CloudProviderTokenResponse { Token = tokenResponse.AccessToken };
        }

        private async Task StoreEntraTokensAsync(Guid userId, int providerId, TokenResponse tokenResponse)
        {
            // Store access token
            await _azureDevOpsService.StoreTokenAsync(
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