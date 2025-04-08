using System.ComponentModel.DataAnnotations;

namespace ReleaseManagerIdentityApi.Models.Entities
{
    public class Role : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(256)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Description { get; set; } = string.Empty;
    }
}
