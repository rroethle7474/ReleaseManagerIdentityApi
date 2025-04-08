namespace ReleaseManagerIdentityApi.Services.DevOpsServices
{
    public interface IAzureDevOpsService
    {
        Task<string> GetAzureDevOpsTokenAsync(Guid userId);
        Task StoreAzureDevOpsTokenAsync(Guid userId, string token, DateTime expiresOn);
        Task<bool> ConnectToAzureDevOpsAsync(Guid userId, string orgName, string personalAccessToken);
    }
}
