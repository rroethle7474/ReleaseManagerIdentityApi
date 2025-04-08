namespace ReleaseManagerIdentityApi.Models.DTOs.Requests
{
    public class ConnectAzureDevOpsRequest
    {
        public string OrganizationName { get; set; }
        public string PersonalAccessToken { get; set; }
    }
}
