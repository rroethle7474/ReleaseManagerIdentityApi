using ReleaseManagerIdentityApi.Models.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReleaseManagerIdentityApi.Models.Entities
{
    [Table("UserToken")]
    public class UserToken : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public int TokenTypeId { get; set; }

        [Column(TypeName = "varchar(MAX)")]
        public string? TokenValue { get; set; }

        [Required]
        public DateTime ExpiresOn { get; set; } = DateTime.UtcNow.AddDays(7);


        [ForeignKey("Id")]
        public virtual User User { get; set; }

        [ForeignKey("Id")]
        public virtual TokenType TokenType { get; set; }
    }
}