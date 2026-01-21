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
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<User>(entity =>
            {
                entity.Property(x => x.FullName).IsRequired().HasMaxLength(100);
                entity.Property(x => x.UserName).IsRequired().HasMaxLength(20);
                entity.Property(x => x.CreatedAt).IsRequired().HasDefaultValueSql("GETUTCDATE()");
                entity.Property(x => x.Balance).IsRequired().HasDefaultValue(0).HasColumnType("decimal(18,2)");
                entity.Property(x => x.LockedBalance).IsRequired().HasDefaultValue(0).HasColumnType("decimal(18,2)");
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
                entity.Property(x => x.PickupCode).IsRequired().HasDefaultValue(0);
                entity.Property(x => x.FailedPickupAttempts).IsRequired().HasDefaultValue(0);
                entity.Property(x => x.IsLocked).IsRequired().HasDefaultValue(false);
                entity.Property(x => x.OriginalEndDate).IsRequired(false);
                entity.Property(x => x.ReturnDate).IsRequired(false);
                entity.Property(x => x.ClosingNote).IsRequired(false).HasMaxLength(1000);

                entity.HasQueryFilter(x => !x.IsDeleted);

                entity.HasOne(x => x.Renter).WithMany(x => x.Bookings).HasForeignKey(x => x.RenterId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(x => x.Tool).WithMany(x => x.Bookings).HasForeignKey(x => x.ToolId).OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Conversation>(entity =>
            {
                entity.Property(x => x.User1Id).IsRequired();
                entity.Property(x => x.User2Id).IsRequired();
                entity.Property(x => x.LastMessageAt).IsRequired();

                entity.HasIndex(x => new {x.User1Id, x.User2Id}).IsUnique();

                entity.HasQueryFilter(x => !x.IsDeleted);

                entity.HasOne(x => x.User1).WithMany().HasForeignKey(x => x.User1Id).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(x => x.User2).WithMany().HasForeignKey(x => x.User2Id).OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Message>(entity =>
            {
                entity.Property(x => x.ConversationId).IsRequired();
                entity.Property(x => x.SenderId).IsRequired();
                entity.Property(x => x.Content).IsRequired();
                entity.Property(x => x.IsRead).IsRequired();
                entity.Property(x => x.CreatedAt).IsRequired();

                entity.HasQueryFilter(x => !x.IsDeleted);

                entity.HasOne(x => x.Conversation).WithMany(x => x.Messages).HasForeignKey(x => x.ConversationId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(x => x.Sender).WithMany().HasForeignKey(x => x.SenderId).OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Rating>(entity =>
            {
                entity.Property(x => x.RatedUserId).IsRequired();
                entity.Property(x => x.RaterUserId).IsRequired();
                entity.Property(x => x.Rate).IsRequired();
                entity.Property(x => x.Comment).IsRequired(false);
                entity.Property(x => x.UpdatedAt).IsRequired();
                entity.Property(x => x.CreatedAt).IsRequired();

                entity.HasKey(x => new {x.RatedUserId, x.RaterUserId});

                entity.HasOne(x => x.RaterUser).WithMany().HasForeignKey(x => x.RaterUserId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(x => x.RatedUser).WithMany().HasForeignKey(x => x.RatedUserId).OnDelete(DeleteBehavior.NoAction);
            });

            builder.Entity<Transaction>(entity =>
            {
                entity.Property(x => x.UserId).IsRequired();
                entity.Property(x => x.Amount).IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(x => x.Type).IsRequired();
                entity.Property(x => x.CreatedAt).IsRequired();
                entity.Property(x => x.Description).IsRequired().HasMaxLength(500);
                entity.Property(x => x.BookingId).IsRequired(false);
                entity.Property(x => x.BalanceSnapshot).IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(x => x.LockedBalanceSnapshot).IsRequired().HasColumnType("decimal(18,2)");

                entity.HasOne(x => x.User).WithMany(x => x.Transactions).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(x => x.Booking).WithMany().HasForeignKey(x => x.BookingId).OnDelete(DeleteBehavior.SetNull);
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
