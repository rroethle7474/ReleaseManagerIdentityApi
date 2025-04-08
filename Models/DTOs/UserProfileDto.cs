namespace ReleaseManagerIdentityApi.Models.DTOs
{
    public class UserProfileDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string AvatarUrl { get; set; }
        public string TimeZone { get; set; }
        public DateTime? LastLoggedInOn { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
