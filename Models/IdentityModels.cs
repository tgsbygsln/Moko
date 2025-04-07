using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AirFastNew.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(u => u.Id).HasMaxLength(128);
                entity.Property(u => u.UserName).HasMaxLength(128);
                entity.Property(u => u.NormalizedUserName).HasMaxLength(128);
                entity.Property(u => u.Email).HasMaxLength(128);
                entity.Property(u => u.NormalizedEmail).HasMaxLength(128);
            });

            modelBuilder.Entity<IdentityRole>(entity =>
            {
                entity.Property(r => r.Id).HasMaxLength(128);
                entity.Property(r => r.Name).HasMaxLength(128);
                entity.Property(r => r.NormalizedName).HasMaxLength(128);
            });
        }
    }
}