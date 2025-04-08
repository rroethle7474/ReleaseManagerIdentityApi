using ReleaseManagerIdentityApi.Models.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReleaseManagerIdentityApi.Models.Entities
{
    [Table("User")]
    public class User : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(256)]
        public string Email { get; set; }

        [Required]
        [StringLength(200)]
        public string Password { get; set; }

        [Required]
        [StringLength(100)]
        public string PasswordSalt { get; set; }

        public bool IsActivated { get; set; }

        public bool IsLocked { get; set; }

        public DateTime? LockoutExpiresOn { get; set; }

        public DateTime? LastLoggedInOn { get; set; }

        public DateTime? LastActivityOn { get; set; }

        public int FailedPasswordAttempts { get; set; }

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(100)]
        public string LastName { get; set; }

        [StringLength(200)]
        public string AvatarUrl { get; set; }

        [StringLength(100)]
        public string TimeZone { get; set; }

        public bool IsInvite { get; set; }
    }
}