using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReleaseManagerIdentityApi.Models.Entities
{
    public class UserRole : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid RoleId { get; set; }

        [ForeignKey("Id")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("Id")]
        public virtual Role Role { get; set; } = null!;
    }
}
