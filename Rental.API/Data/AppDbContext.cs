using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Rental.API.Common;
using Rental.API.Models;

namespace Rental.API.Data
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base (options)
        {
            
        }

        public DbSet<Tool> Tools { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<User>(entity =>
            {
                entity.Property(x => x.FullName).IsRequired().HasMaxLength(100);
                entity.Property(x => x.UserName).IsRequired().HasMaxLength(20);
                entity.Property(x => x.CreatedAt).IsRequired().HasDefaultValueSql("GETUTCDATE()");
            });
            builder.Entity<User>().HasQueryFilter(x => !x.IsDeleted);

            builder.Entity<Tool>(entity =>
            {
                entity.Property(x => x.Name).IsRequired().HasMaxLength(100);
                entity.Property(x => x.DailyPrice).IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(x => x.IsActive).IsRequired();
                entity.Property(x => x.UserId).IsRequired();
                entity.Property(x => x.SecurityDeposit).IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(x => x.Description).IsRequired().HasMaxLength(1000);
                entity.Property(x => x.Category).IsRequired().HasConversion<string>();
                entity.Property(x => x.Location).IsRequired().HasMaxLength(200);
                entity.Property(x => x.AvailableUntil).IsRequired().HasDefaultValueSql("CAST(GETUTCDATE() AS DATE)");

                entity.HasOne(x => x.User).WithMany(x => x.Tools).HasForeignKey(x => x.UserId);
            });
            builder.Entity<Tool>().HasQueryFilter(x => !x.IsDeleted);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries<ISoftDelete>()
                                       .Where(e => e.State == EntityState.Deleted);

            foreach (var entry in entries)
            {
                entry.State = EntityState.Modified;

                entry.Entity.IsDeleted = true;
                entry.Entity.DeletedAt = DateTime.UtcNow;
            }

            return base.SaveChangesAsync(cancellationToken);
        }

    }
}
