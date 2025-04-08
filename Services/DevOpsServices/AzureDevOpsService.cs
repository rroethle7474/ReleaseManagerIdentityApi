using Microsoft.EntityFrameworkCore;
using ReleaseManagerIdentityApi.Data;
using ReleaseManagerIdentityApi.Models.Entities;

namespace ReleaseManagerIdentityApi.Services.DevOpsServices
{
    public class AzureDevOpsService : IAzureDevOpsService
    {
        private readonly ApplicationDbContext _context;

        public AzureDevOpsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<string> GetAzureDevOpsTokenAsync(Guid userId)
        {
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

            return token?.TokenValue;
        }

        public async Task StoreAzureDevOpsTokenAsync(Guid userId, string token, DateTime expiresOn)
        {
            // Find the Azure DevOps token type
            var tokenType = await _context.TokenTypes
                .FirstOrDefaultAsync(t => t.Name == "AzureDevOpsToken");

            if (tokenType == null)
            {
                // Create the token type if it doesn't exist
                tokenType = new TokenType
                {
                    Id = 2, // Assuming 1 is already used for RefreshToken
                    Name = "AzureDevOpsToken",
                    Description = "Token for Azure DevOps integration",
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = Guid.Parse("09072755-8B7D-49BB-B967-12598B091971"), // System user ID
                    UpdatedOn = DateTime.UtcNow,
                    UpdatedBy = Guid.Parse("09072755-8B7D-49BB-B967-12598B091971")
                };

                _context.TokenTypes.Add(tokenType);
                await _context.SaveChangesAsync();
            }

            // Check if user already has an Azure DevOps token
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

        public async Task<bool> ConnectToAzureDevOpsAsync(Guid userId, string orgName, string personalAccessToken)
        {
            // This is a simple implementation. In a real-world scenario, you would:
            // 1. Validate the PAT with Azure DevOps API
            // 2. Store organization details in DevOpsOrganization table
            // 3. Store the PAT securely

            try
            {
                // Get user's organization
                var orgUser = await _context.OrganizationUsers
                .FirstOrDefaultAsync(ou => ou.UserId == userId);

                if (orgUser == null)
                {
                    throw new InvalidOperationException("User is not associated with any organization");
                }

                // Check if DevOps organization already exists
                var existingDevOpsOrg = await _context.Set<DevOpsOrganization>()
                .FirstOrDefaultAsync(d => d.OrganizationId == orgUser.OrganizationId &&
                                             d.DevOpsOrgName == orgName);

                if (existingDevOpsOrg == null)
                {
                    // Create new DevOps organization
                    var devOpsOrg = new DevOpsOrganization
                    {
                        Id = Guid.NewGuid(),
                        OrganizationId = orgUser.OrganizationId,
                        DevOpsOrgName = orgName,
                        UserId = userId,
                        AzureDevOpsOrgIdentifier = orgName,
                        CreatedOn = DateTime.UtcNow,
                        CreatedBy = userId,
                        UpdatedOn = DateTime.UtcNow,
                        UpdatedBy = userId
                    };

                    _context.Set<DevOpsOrganization>().Add(devOpsOrg);
                }

                // Store the PAT (in a real implementation, consider encrypting this)
                await StoreAzureDevOpsTokenAsync(
                    userId,
                    personalAccessToken,
                    DateTime.UtcNow.AddDays(90) // PATs can have different expirations
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