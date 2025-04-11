using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ReleaseManagerIdentityApi.Models.Entities
{
    public class CloudOrganization : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }

        public Guid OrganizationId { get; set; }

        public int CloudProviderId { get; set; }

        [Required]
        [StringLength(200)]
        public string OrganizationName { get; set; }

        public Guid? UserId { get; set; }

        [StringLength(200)]
        public string? ProviderIdentifier { get; set; }

        public int AuthMethodId { get; set; }

        [ForeignKey("OrganizationId")]
        public virtual Organization Organization { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [ForeignKey("CloudProviderId")]
        public virtual CloudProvider CloudProvider { get; set; }

        [ForeignKey("AuthMethodId")]
        public virtual AuthMethod AuthMethod { get; set; }
    }
}
