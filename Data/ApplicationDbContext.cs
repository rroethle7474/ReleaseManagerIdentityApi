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

            modelBuilder.Entity<User>().ToTable("User");
            modelBuilder.Entity<Organization>().ToTable("Organization");
            modelBuilder.Entity<TokenType>().ToTable("TokenType");
            modelBuilder.Entity<UserToken>().ToTable("UserToken");
            modelBuilder.Entity<Role>().ToTable("Role");
            modelBuilder.Entity<UserRole>().ToTable("UserRole");
            modelBuilder.Entity<OrganizationUser>().ToTable("OrganizationUser");

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

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany()
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany()
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrganizationUser>()
                .HasOne(ou => ou.User)
                .WithMany()
                .HasForeignKey(ou => ou.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrganizationUser>()
                .HasOne(ou => ou.Organization)
                .WithMany()
                .HasForeignKey(ou => ou.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}