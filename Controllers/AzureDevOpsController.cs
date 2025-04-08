using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReleaseManagerIdentityApi.Models.DTOs.Requests;
using ReleaseManagerIdentityApi.Models.DTOs.Responses;
using ReleaseManagerIdentityApi.Services.DevOpsServices;
using System.Security.Claims;

namespace ReleaseManagerIdentityApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AzureDevOpsController : ControllerBase
    {
        private readonly IAzureDevOpsService _azureDevOpsService;

        public AzureDevOpsController(IAzureDevOpsService azureDevOpsService)
        {
            _azureDevOpsService = azureDevOpsService;
        }

        [HttpPost("connect")]
        public async Task<IActionResult> ConnectAzureDevOps(ConnectAzureDevOpsRequest request)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                var result = await _azureDevOpsService.ConnectToAzureDevOpsAsync(
                    userId,
                    request.OrganizationName,
                    request.PersonalAccessToken
                );

                if (result)
                {
                    return Ok(new { message = "Connected to Azure DevOps organization successfully" });
                }

                return BadRequest(new { message = "Failed to connect to Azure DevOps" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred while connecting to Azure DevOps" });
            }
        }

        [HttpGet("token")]
        public async Task<ActionResult<AzureDevOpsTokenResponse>> GetAzureDevOpsToken()
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                var token = await _azureDevOpsService.GetAzureDevOpsTokenAsync(userId);

                if (string.IsNullOrEmpty(token))
                {
                    return NotFound(new { message = "Azure DevOps token not found" });
                }

                return Ok(new AzureDevOpsTokenResponse { Token = token });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving Azure DevOps token" });
            }
        }
    }
}