using Microsoft.EntityFrameworkCore;
using ReleaseManagerIdentityApi.Data;
using ReleaseManagerIdentityApi.Models.DTOs;
using ReleaseManagerIdentityApi.Models.DTOs.Requests;
using ReleaseManagerIdentityApi.Models.Entities;
using ReleaseManagerIdentityApi.Utilities;

namespace ReleaseManagerIdentityApi.Services.OrganizationService
{
    public class OrganizationService : IOrganizationService
    {
        private readonly ApplicationDbContext _context;

        public OrganizationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<OrganizationDto> GetUserOrganizationAsync(Guid userId)
        {
            var orgUser = await _context.OrganizationUsers
                .Include(ou => ou.Organization)
                .FirstOrDefaultAsync(ou => ou.UserId == userId);

            if (orgUser == null)
            {
                throw new InvalidOperationException("User is not associated with any organization");
            }

            return new OrganizationDto
            {
                Id = orgUser.Organization.Id,
                Name = orgUser.Organization.Name,
                OrganizationLogo = orgUser.Organization.OrganizationLogo,
                CreatedOn = orgUser.Organization.CreatedOn
            };
        }

        public async Task<bool> IsUserOrganizationAdminAsync(Guid userId, Guid organizationId)
        {
            var orgUser = await _context.OrganizationUsers
                .FirstOrDefaultAsync(ou => ou.UserId == userId && ou.OrganizationId == organizationId);

            return orgUser?.IsAdmin ?? false;
        }

        public async Task<bool> CanUserInviteAsync(Guid userId, Guid organizationId)
        {
            var orgUser = await _context.OrganizationUsers
                .FirstOrDefaultAsync(ou => ou.UserId == userId && ou.OrganizationId == organizationId);

            return orgUser?.CanInvite ?? false;
        }

        public async Task UpdateOrganizationAsync(Guid organizationId, UpdateOrganizationRequest request)
        {
            var organization = await _context.Organizations.FindAsync(organizationId);

            if (organization == null)
            {
                throw new InvalidOperationException("Organization not found");
            }

            // Update organization properties
            if (!string.IsNullOrEmpty(request.Name))
                organization.Name = request.Name;

            if (!string.IsNullOrEmpty(request.OrganizationLogo))
                organization.OrganizationLogo = request.OrganizationLogo;

            organization.UpdatedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task<List<OrganizationUserDto>> GetOrganizationUsersAsync(Guid organizationId)
        {
            var orgUsers = await _context.OrganizationUsers
                .Include(ou => ou.User)
                .Where(ou => ou.OrganizationId == organizationId)
                .ToListAsync();

            return orgUsers.Select(ou => new OrganizationUserDto
            {
                Id = ou.Id,
                UserId = ou.UserId,
                Email = ou.User.Email,
                FirstName = ou.User.FirstName,
                LastName = ou.User.LastName,
                AvatarUrl = ou.User.AvatarUrl,
                IsAdmin = ou.IsAdmin,
                IsBilling = ou.IsBilling,
                CanInvite = ou.CanInvite,
                CreatedOn = ou.CreatedOn
            }).ToList();
        }

        public async Task InviteUserAsync(Guid organizationId, InviteUserRequest request, Guid invitedBy)
        {
            // Check if user already exists
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            Guid userId;

            if (existingUser == null)
            {
                // Create a new user with invite flag
                var salt = SecurityUtilities.GenerateSalt();
                var tempPassword = SecurityUtilities.GenerateRandomPassword();
                var hashedPassword = SecurityUtilities.HashPassword(tempPassword, salt);

                userId = Guid.NewGuid();
                var user = new User
                {
                    Id = userId,
                    Email = request.Email,
                    Password = hashedPassword,
                    PasswordSalt = salt,
                    FirstName = "Invited",
                    LastName = "User",
                    IsActivated = false,
                    IsLocked = false,
                    IsInvite = true,
                    FailedPasswordAttempts = 0,
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = invitedBy,
                    UpdatedOn = DateTime.UtcNow,
                    UpdatedBy = invitedBy
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }
            else
            {
                userId = existingUser.Id;

                // Check if user is already part of this organization
                var existingOrgUser = await _context.OrganizationUsers
                    .FirstOrDefaultAsync(ou => ou.UserId == userId && ou.OrganizationId == organizationId);

                if (existingOrgUser != null)
                {
                    throw new InvalidOperationException("User is already a member of this organization");
                }
            }

            // Create organization user relationship
            var organizationUser = new OrganizationUser
            {
                Id = Guid.NewGuid(),
                OrganizationId = organizationId,
                UserId = userId,
                IsAdmin = request.IsAdmin,
                IsBilling = request.IsBilling,
                CanInvite = request.CanInvite,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = invitedBy,
                UpdatedOn = DateTime.UtcNow,
                UpdatedBy = invitedBy
            };

            _context.OrganizationUsers.Add(organizationUser);
            await _context.SaveChangesAsync();

            // TODO: Send invitation email to user
        }
    }
}