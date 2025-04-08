namespace ReleaseManagerIdentityApi.Models.DTOs
{
    public class OrganizationUserDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string AvatarUrl { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsBilling { get; set; }
        public bool CanInvite { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}