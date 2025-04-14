// In a new controller, e.g., EntraAuthController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ReleaseManagerIdentityApi.Models.DTOs.Requests;
using ReleaseManagerIdentityApi.Models.DTOs.Responses;
using ReleaseManagerIdentityApi.Models.Entities;
using ReleaseManagerIdentityApi.Services.Auth;
using ReleaseManagerIdentityApi.Services.DevOpsServices;
using System.Security.Claims;

[Route("api/[controller]")]
[ApiController]
public class MicrosoftEntraAuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IEntraTokenExchangeService _tokenExchangeService;
    private readonly ICloudProviderService _cloudProviderService;
    private readonly IConfiguration _configuration;


    public MicrosoftEntraAuthController(IAuthService authService, IEntraTokenExchangeService tokenExchangeService, 
        ICloudProviderService cloudProviderService,
        IConfiguration configuration)
    {
        _authService = authService;
        _tokenExchangeService = tokenExchangeService;
        _cloudProviderService = cloudProviderService;
        _configuration = configuration;
    }

    [HttpGet("connect-azure-devops")]
    public IActionResult ConnectAzureDevOps()
    {
        var tenantId = _configuration["MicrosoftEntra:TenantId"];
        var clientId = _configuration["MicrosoftEntra:ClientId"];
        var redirectUri = _configuration["MicrosoftEntra:RedirectUri"];
        var scope = "499b84ac-1321-427f-aa17-267ca6975798/.default"; // Azure DevOps resource ID

        var authUrl = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/authorize" +
                      $"?client_id={clientId}" +
                      $"&response_type=code" +
                      $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                      $"&scope={Uri.EscapeDataString(scope)}" +
                      $"&response_mode=query";

        return Redirect(authUrl);
    }

    [HttpGet("oauth-callback")]
    public async Task<IActionResult> OAuthCallback([FromQuery] string code)
    {
        try
        {
            var tenantId = _configuration["MicrosoftEntra:TenantId"];
            var clientId = _configuration["MicrosoftEntra:ClientId"];
            var clientSecret = _configuration["MicrosoftEntra:ClientSecret"];
            var redirectUri = _configuration["MicrosoftEntra:RedirectUri"];

            // Exchange auth code for token
            var tokenResponse = await _tokenExchangeService.ExchangeCodeForTokenAsync(
                code, clientId, clientSecret, redirectUri, tenantId);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Store the token
            await _cloudProviderService.StoreTokenAsync(
                Guid.Parse(userId),
                1, // Azure DevOps provider ID
                tokenResponse.AccessToken,
                DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn),
                2, // OAuth auth method
                true); // Flag as Entra token

            return RedirectToAction("Index", "Home", new { message = "Successfully connected to Azure DevOps" });
        }
        catch (Exception ex)
        {
            return RedirectToAction("Error", "Home", new { message = ex.Message });
        }
    }

    [HttpPost("refresh-entra-token")]
    public async Task<ActionResult<EntraAuthResponse>> RefreshEntraToken([FromBody] RefreshEntraTokenRequest request)
    {
        try
        {
            var result = await _tokenExchangeService.RefreshEntraTokenAsync(request.Userid, request.providerId);
            var authResponse = new EntraAuthResponse { AccessToken = result.Token };
            return Ok(result);
        }
        catch (SecurityTokenException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while refreshing token" });
        }
    }
}