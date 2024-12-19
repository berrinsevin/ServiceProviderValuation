using Microsoft.EntityFrameworkCore;
using ServiceProviderRatingNuget.Domain.Entities;

namespace ServiceProviderRatingNuget.DataAccess
{
    public class ServiceProviderRatingDbContext : DbContext
    {
        public ServiceProviderRatingDbContext(DbContextOptions<ServiceProviderRatingDbContext> options) : base(options)
        {
        }

        public DbSet<Rating> Ratings { get; set; }
        public DbSet<Provider> ServiceProviders { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Rating>()
                .HasKey(r => r.Id);

            modelBuilder.Entity<Provider>()
                .HasKey(p => p.Id);

            modelBuilder.Entity<User>()
                .HasKey(u => u.Id);

            modelBuilder.Entity<Notification>()
                .HasKey(n => n.Id);

            modelBuilder.Entity<Provider>()
                .HasMany(p => p.Ratings)
                .WithOne(r => r.Provider)
                .HasForeignKey(r => r.ServiceProviderId);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Ratings)
                .WithOne(r => r.User)
                .HasForeignKey(r => r.UserId);
        }
    }
}
