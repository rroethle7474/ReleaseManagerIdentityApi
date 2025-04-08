using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReleaseManagerIdentityApi.Models.Entities
{
    public class OrganizationUser : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }
        public Guid OrganizationId { get; set; }
        public Guid UserId { get; set; } // Foreign key to the User entity
        public bool IsAdmin { get; set; } = true;
        public bool IsBilling { get; set; } = false;
        public bool CanInvite { get; set; } = false;

        [ForeignKey("Id")]
        public virtual Organization Organization { get; set; }

        [ForeignKey("Id")]
        public virtual User User { get; set; }

    }
}
