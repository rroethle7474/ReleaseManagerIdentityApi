using ReleaseManagerIdentityApi.Models.DTOs;
using ReleaseManagerIdentityApi.Models.DTOs.Requests;

namespace ReleaseManagerIdentityApi.Services.OrganizationService
{
    public interface IOrganizationService
    {
        Task<OrganizationDto> GetUserOrganizationAsync(Guid userId);
        Task<bool> IsUserOrganizationAdminAsync(Guid userId, Guid organizationId);
        Task<bool> CanUserInviteAsync(Guid userId, Guid organizationId);
        Task UpdateOrganizationAsync(Guid organizationId, UpdateOrganizationRequest request);
        Task<List<OrganizationUserDto>> GetOrganizationUsersAsync(Guid organizationId);
        Task InviteUserAsync(Guid organizationId, InviteUserRequest request, Guid invitedBy);
    }
}
