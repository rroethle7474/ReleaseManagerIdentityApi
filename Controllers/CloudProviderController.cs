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
    public class CloudProviderController : ControllerBase
    {
        private readonly ICloudProviderService _cloudProviderService;

        public CloudProviderController(ICloudProviderService cloudProviderService)
        {
            _cloudProviderService = cloudProviderService;
        }

        [HttpPost("connect")]
        public async Task<IActionResult> ConnectProvider([FromBody] ConnectCloudProviderRequest request)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                var result = await _cloudProviderService.ConnectAsync(
                    userId,
                    request.CloudProviderId,
                    request.OrganizationName,
                    request.Token,
                    request.AuthMethodId
                );

                if (result)
                {
                    return Ok(new { message = "Connected to cloud provider successfully" });
                }

                return BadRequest(new { message = "Failed to connect to cloud provider" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred while connecting to cloud provider" });
            }
        }

        [HttpGet("token/{cloudProviderId}")]
        public async Task<ActionResult<CloudProviderTokenResponse>> GetToken(int cloudProviderId)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                var token = await _cloudProviderService.GetTokenAsync(userId, cloudProviderId);

                if (string.IsNullOrEmpty(token))
                {
                    return NotFound(new { message = "Token not found" });
                }

                return Ok(new CloudProviderTokenResponse { Token = token });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving token" });
            }
        }
    }
}