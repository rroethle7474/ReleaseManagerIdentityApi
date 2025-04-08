namespace ReleaseManagerIdentityApi.Models.Entities
{
    public class BaseEntity
    {
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedOn { get; set; } = DateTime.UtcNow;
        public Guid CreatedBy { get; set; }
        public Guid UpdatedBy { get; set; }
    }
}
