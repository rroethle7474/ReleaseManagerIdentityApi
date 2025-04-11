using Microsoft.EntityFrameworkCore;
using ReleaseManagerIdentityApi.Data;
using ReleaseManagerIdentityApi.Models.Entities;

namespace ReleaseManagerIdentityApi.Services.DevOpsServices
{
    public class AzureDevOpsService : ICloudProviderService
    {
        private readonly ApplicationDbContext _context;
        private const int AZURE_DEVOPS_PROVIDER_ID = 1; // As defined in your CloudProvider table

        public AzureDevOpsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<string> GetTokenAsync(Guid userId, int cloudProviderId)
        {
            // Ensure we're dealing with Azure DevOps
            if (cloudProviderId != AZURE_DEVOPS_PROVIDER_ID)
            {
                throw new InvalidOperationException("This service only handles Azure DevOps tokens");
            }

            // Find the Azure DevOps token type
            var tokenType = await _context.TokenTypes
                .FirstOrDefaultAsync(t => t.Name == "AzureDevOpsToken");

            if (tokenType == null)
            {
                throw new InvalidOperationException("Azure DevOps token type not found");
            }

            // Get the user's Azure DevOps token
            var token = await _context.UserTokens
                .FirstOrDefaultAsync(t => t.UserId == userId &&
                                         t.TokenTypeId == tokenType.Id &&
                                         t.ExpiresOn > DateTime.UtcNow);

            if(String.IsNullOrEmpty(token?.TokenValue))
            {
                throw new InvalidOperationException("No valid Azure DevOps token found for the user");
            }

            return token.TokenValue;
        }

        public async Task StoreTokenAsync(Guid userId, int cloudProviderId, string token, DateTime expiresOn, int authMethodId)
        {
            // Ensure we're dealing with Azure DevOps
            if (cloudProviderId != AZURE_DEVOPS_PROVIDER_ID)
            {
                throw new InvalidOperationException("This service only handles Azure DevOps tokens");
            }

            // Determine the correct token type based on auth method
            string tokenTypeName = authMethodId == 1 ? "AzureDevOpsToken" : "OAuthRefreshToken";

            // Find the token type
            var tokenType = await _context.TokenTypes
                .FirstOrDefaultAsync(t => t.Name == tokenTypeName);

            if (tokenType == null)
            {
                throw new InvalidOperationException($"{tokenTypeName} token type not found");
            }

            // Check if user already has this type of token
            var existingToken = await _context.UserTokens
                .FirstOrDefaultAsync(t => t.UserId == userId && t.TokenTypeId == tokenType.Id);

            if (existingToken != null)
            {
                // Update existing token
                existingToken.TokenValue = token;
                existingToken.ExpiresOn = expiresOn;
                existingToken.UpdatedOn = DateTime.UtcNow;
                existingToken.UpdatedBy = userId;
            }
            else
            {
                // Create new token
                var userToken = new UserToken
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    TokenTypeId = tokenType.Id,
                    TokenValue = token,
                    ExpiresOn = expiresOn,
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = userId,
                    UpdatedOn = DateTime.UtcNow,
                    UpdatedBy = userId
                };

                _context.UserTokens.Add(userToken);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<bool> ConnectAsync(Guid userId, int cloudProviderId, string organizationName, string token, int authMethodId)
        {
            try
            {
                // Ensure we're dealing with Azure DevOps
                if (cloudProviderId != AZURE_DEVOPS_PROVIDER_ID)
                {
                    throw new InvalidOperationException("This service only handles Azure DevOps connections");
                }

                // Get user's organization
                var orgUser = await _context.OrganizationUsers
                    .FirstOrDefaultAsync(ou => ou.UserId == userId);

                if (orgUser == null)
                {
                    throw new InvalidOperationException("User is not associated with any organization");
                }

                // Check if cloud organization already exists
                var existingCloudOrg = await _context.CloudOrganizations
                    .FirstOrDefaultAsync(co => co.OrganizationId == orgUser.OrganizationId &&
                                             co.CloudProviderId == cloudProviderId &&
                                             co.OrganizationName == organizationName);

                if (existingCloudOrg == null)
                {
                    // Create new cloud organization
                    var cloudOrg = new CloudOrganization
                    {
                        Id = Guid.NewGuid(),
                        OrganizationId = orgUser.OrganizationId,
                        CloudProviderId = cloudProviderId,
                        OrganizationName = organizationName,
                        UserId = userId,
                        ProviderIdentifier = organizationName,
                        AuthMethodId = authMethodId,
                        CreatedOn = DateTime.UtcNow,
                        CreatedBy = userId,
                        UpdatedOn = DateTime.UtcNow,
                        UpdatedBy = userId
                    };

                    _context.CloudOrganizations.Add(cloudOrg);
                }

                // Store the token
                await StoreTokenAsync(
                    userId,
                    cloudProviderId,
                    token,
                    DateTime.UtcNow.AddDays(authMethodId == 1 ? 90 : 30), // PAT vs OAuth different expirations
                    authMethodId
                );

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}