using Microsoft.EntityFrameworkCore;
using ReleaseManagerIdentityApi.Models.Entities;

namespace ReleaseManagerIdentityApi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<TokenType> TokenTypes { get; set; }
        public DbSet<UserToken> UserTokens { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<OrganizationUser> OrganizationUsers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure table names
            modelBuilder.Entity<User>().ToTable("User");
            modelBuilder.Entity<Organization>().ToTable("Organization");
            modelBuilder.Entity<TokenType>().ToTable("TokenType");
            modelBuilder.Entity<UserToken>().ToTable("UserToken");
            modelBuilder.Entity<OrganizationUser>().ToTable("OrganizationUser");
            modelBuilder.Entity<Role>().ToTable("Role");
            modelBuilder.Entity<UserRole>().ToTable("UserRole");

            // Configure relationships
            modelBuilder.Entity<UserToken>()
                .HasOne(ut => ut.User)
                .WithMany()
                .HasForeignKey(ut => ut.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserToken>()
                .HasOne(ut => ut.TokenType)
                .WithMany()
                .HasForeignKey(ut => ut.TokenTypeId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}