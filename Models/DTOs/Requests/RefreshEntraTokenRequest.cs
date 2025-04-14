namespace ReleaseManagerIdentityApi.Models.DTOs.Requests
{
    public class RefreshEntraTokenRequest
    {
        public Guid Userid { get; set; }
        public int providerId { get; set; }
    }
}
