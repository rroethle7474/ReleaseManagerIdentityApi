using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReleaseManagerIdentityApi.Models.DTOs;
using ReleaseManagerIdentityApi.Models.DTOs.Requests;
using ReleaseManagerIdentityApi.Services.OrganizationService;
using System.Security.Claims;

namespace ReleaseManagerIdentityApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class OrganizationsController : ControllerBase
    {
        private readonly IOrganizationService _organizationService;

        public OrganizationsController(IOrganizationService organizationService)
        {
            _organizationService = organizationService;
        }

        [HttpGet("current")]
        public async Task<ActionResult<OrganizationDto>> GetCurrentOrganization()
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var organization = await _organizationService.GetUserOrganizationAsync(userId);
                return Ok(organization);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving organization" });
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateOrganization(UpdateOrganizationRequest request)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var organizationId = Guid.Parse(User.FindFirst("organizationId")?.Value);

                // Check if user has permission to update organization
                var isAdmin = await _organizationService.IsUserOrganizationAdminAsync(userId, organizationId);
                if (!isAdmin)
                {
                    return Forbid();
                }

                await _organizationService.UpdateOrganizationAsync(organizationId, request);
                return Ok(new { message = "Organization updated successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred while updating organization" });
            }
        }

        [HttpGet("users")]
        public async Task<ActionResult> GetOrganizationUsers()
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var organizationId = Guid.Parse(User.FindFirst("organizationId")?.Value);

                var users = await _organizationService.GetOrganizationUsersAsync(organizationId);
                return Ok(users);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving organization users" });
            }
        }

        [HttpPost("users/invite")]
        public async Task<IActionResult> InviteUser(InviteUserRequest request)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var organizationId = Guid.Parse(User.FindFirst("organizationId")?.Value);

                // Check if user has permission to invite
                var canInvite = await _organizationService.CanUserInviteAsync(userId, organizationId);
                if (!canInvite)
                {
                    return Forbid();
                }

                await _organizationService.InviteUserAsync(organizationId, request, userId);
                return Ok(new { message = "User invited successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred while inviting user" });
            }
        }
    }
}