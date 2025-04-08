using ReleaseManagerIdentityApi.Models.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReleaseManagerIdentityApi.Models.Entities
{
    [Table("Organization")]
    public class Organization : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(400)]
        public string OrganizationLogo { get; set; }
    }
}