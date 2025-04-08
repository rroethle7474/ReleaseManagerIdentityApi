namespace ReleaseManagerIdentityApi.Models.DTOs
{
    public class OrganizationDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string OrganizationLogo { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
