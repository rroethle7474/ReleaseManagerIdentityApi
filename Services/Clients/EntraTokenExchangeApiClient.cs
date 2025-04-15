using ReleaseManagerIdentityApi.Models.DTOs.Responses;
using System.Net.Http;
using System.Text.Json;

namespace ReleaseManagerIdentityApi.Services.Clients
{
    public class EntraTokenExchangeApiClient : IEntraTokenExchangeApiClient
    {
        private readonly HttpClient _httpClient;

        public EntraTokenExchangeApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
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
                $"{tenantId}/oauth2/v2.0/token",
                requestContent);

            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TokenResponse>(responseContent);
        }

        public async Task<TokenResponse> RefreshTokenAsync(
            string refreshToken, string clientId, string clientSecret, string tenantId)
        {
            var requestContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret),
                new KeyValuePair<string, string>("refresh_token", refreshToken),
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("scope", "499b84ac-1321-427f-aa17-267ca6975798/.default")
            });

            var response = await _httpClient.PostAsync(
                $"{tenantId}/oauth2/v2.0/token",
                requestContent);

            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TokenResponse>(responseContent);
        }
    }
}
