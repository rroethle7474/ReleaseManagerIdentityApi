namespace ReleaseManagerIdentityApi.Models.DTOs.Requests
{
    public class ConnectCloudProviderRequest
    {
        public int CloudProviderId { get; set; }
        public string OrganizationName { get; set; }
        public string Token { get; set; }
        public int AuthMethodId { get; set; }
    }
}
