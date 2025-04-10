using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ReleaseManagerIdentityApi.Models.Entities
{
    [Table("DevOpsOrganization")]
    public class DevOpsOrganization : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }

        public Guid OrganizationId { get; set; }

        [Required]
        [StringLength(200)]
        public string DevOpsOrgName { get; set; }

        public Guid? UserId { get; set; }

        [StringLength(200)]
        public string? AzureDevOpsOrgIdentifier { get; set; }

        [ForeignKey("OrganizationId")]
        public virtual Organization Organization { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}
