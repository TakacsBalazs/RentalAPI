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
        public DbSet<ToolUnavailability> ToolUnavailabilities { get; set; }
        public DbSet<Booking> Bookings { get; set; }

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

            builder.Entity<ToolUnavailability>(entity =>
            {
                entity.Property(x => x.StartDate).IsRequired();
                entity.Property(x => x.EndDate).IsRequired();
                entity.Property(x => x.ToolId).IsRequired();

                entity.HasOne(x => x.Tool).WithMany(x => x.ToolUnavailabilities).HasForeignKey(x => x.ToolId).OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<Booking>(entity =>
            {
                entity.Property(x => x.StartDate).IsRequired();
                entity.Property(x => x.EndDate).IsRequired();
                entity.Property(x => x.Status).IsRequired().HasConversion<string>();
                entity.Property(x => x.ToolId).IsRequired();
                entity.Property(x => x.RenterId).IsRequired();
                entity.Property(x => x.TotalPrice).IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(x => x.SecurityDeposit).IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(x => x.CreatedAt).IsRequired();

                entity.HasQueryFilter(x => !x.IsDeleted);

                entity.HasOne(x => x.Renter).WithMany(x => x.Bookings).HasForeignKey(x => x.RenterId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(x => x.Tool).WithMany(x => x.Bookings).HasForeignKey(x => x.ToolId).OnDelete(DeleteBehavior.Restrict);
            });
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
