namespace ReleaseManagerIdentityApi.Models.DTOs.Requests
{
    public class UpdateProfileRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string TimeZone { get; set; }
        public string AvatarUrl { get; set; }
    }
}
