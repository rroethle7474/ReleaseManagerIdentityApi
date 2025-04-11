using System.ComponentModel.DataAnnotations;

namespace ReleaseManagerIdentityApi.Models.Entities
{
    public class AuthMethod : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [StringLength(200)]
        public string? Description { get; set; }
    }
}
