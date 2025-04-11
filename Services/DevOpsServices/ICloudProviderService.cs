namespace ReleaseManagerIdentityApi.Services.DevOpsServices
{
    public interface ICloudProviderService
    {
        Task<string> GetTokenAsync(Guid userId, int cloudProviderId);
        Task StoreTokenAsync(Guid userId, int cloudProviderId, string token, DateTime expiresOn, int authMethodId);
        Task<bool> ConnectAsync(Guid userId, int cloudProviderId, string organizationName, string token, int authMethodId);
    }
}
