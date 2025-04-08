namespace ReleaseManagerIdentityApi.Models.DTOs.Requests
{
    public class InviteUserRequest
    {
        public string Email { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsBilling { get; set; }
        public bool CanInvite { get; set; }
    }
}
