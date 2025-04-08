namespace ReleaseManagerIdentityApi.Models.DTOs
{
    public class AuthResponse
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public DateTime Expiration { get; set; }
        public Guid UserId { get; set; }
        public Guid OrganizationId { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}