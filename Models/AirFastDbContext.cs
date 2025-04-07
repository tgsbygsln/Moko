using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace AirFastNew.Models
{
    public class AirFastDbContext : IdentityDbContext<ApplicationUser>
    {
        public AirFastDbContext(DbContextOptions<AirFastDbContext> options)
            : base(options)
        {
        }

        public DbSet<Test> Tests { get; set; }
                public DbSet<Post> Posts { get; set; }
       

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Ensure Identity models are configured

            // Fix MySQL case sensitivity & Identity column length issues
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