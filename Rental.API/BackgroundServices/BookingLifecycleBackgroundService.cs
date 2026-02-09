
using Microsoft.EntityFrameworkCore;
using Rental.API.Data;
using Rental.API.Models;
using Rental.API.Services;

namespace Rental.API.BackgroundServices
{
    public class BookingLifecycleBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory scopeFactory;
        private readonly ILogger<BookingLifecycleBackgroundService> logger;
        public BookingLifecycleBackgroundService(IServiceScopeFactory scopeFactory, ILogger<BookingLifecycleBackgroundService> logger)
        {
            this.scopeFactory = scopeFactory;
            this.logger = logger;

        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            int notificationHour = 8;
            int cleanUpHour = 0;
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.UtcNow;
                var nextNotification = now.Date.AddHours(notificationHour);
                if (now >= nextNotification) {
                    nextNotification = nextNotification.AddDays(1);
                }

                var nextCleanUpBookingAndNotifications = now.Date.AddHours(cleanUpHour);
                if(now >= nextCleanUpBookingAndNotifications)
                {
                    nextCleanUpBookingAndNotifications = nextCleanUpBookingAndNotifications.AddDays(1);
                }

                DateTime nextRunTime;
                if(nextNotification < nextCleanUpBookingAndNotifications)
                {
                    nextRunTime = nextNotification;
                    nextRunTime = nextNotification;
                }
                else
                {
                    nextRunTime = nextCleanUpBookingAndNotifications;
                }

                var delay = nextRunTime - now;
                await Task.Delay(delay.Add(TimeSpan.FromMinutes(1)), stoppingToken);

                try 
                {
                    using (var scope = scopeFactory.CreateScope())
                    {
                        int hour = DateTime.UtcNow.Hour;
                        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                        if (hour == notificationHour)
                        {
                            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                            var today = DateOnly.FromDateTime(DateTime.UtcNow);
                            var bookings = await context.Bookings.Include(x => x.Tool).Where(x => (x.StartDate == today && x.Status == BookingStatus.Reserved) || (x.EndDate == today && x.Status == BookingStatus.Active)).ToListAsync();
                            foreach (var booking in bookings)
                            {
                                if (booking.StartDate == today && booking.EndDate == today)
                                {
                                    await notificationService.SendNotificationAsync(booking.RenterId, "Quick Rental Today!", $"Your booking for {booking.Tool.Name} is today! Pickup Code: {booking.PickupCode}. Please return it by evening.");
                                    await notificationService.SendNotificationAsync(booking.Tool.UserId, "1-Day Rental Today!", $"Your {booking.Tool.Name} is booked for today only. Prepare for pickup and return.");
                                }
                                else if (booking.StartDate == today)
                                {
                                    await notificationService.SendNotificationAsync(booking.RenterId, "Booking Starts Today!", $"Your rental for {booking.Tool.Name} starts today! Pickup Code: {booking.PickupCode}.!");
                                    await notificationService.SendNotificationAsync(booking.Tool.UserId, "Rental Starting!", $"Your {booking.Tool.Name} is scheduled for pickup today. Please ensure it's ready.");
                                }
                                else if (booking.EndDate == today)
                                {
                                    await notificationService.SendNotificationAsync(booking.RenterId, "Booking Ends Today!", $"Please return the {booking.Tool.Name} by the end of the day.");
                                    await notificationService.SendNotificationAsync(booking.Tool.UserId, "Tool Returning Today!", $"Your {booking.Tool.Name} is expected back today. Check for damages upon return.");
                                }
                            }
                        } else if(hour == cleanUpHour)
                        {
                            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
                            var bookings = await context.Bookings.Where(x => x.Status == BookingStatus.Reserved && DateOnly.FromDateTime(DateTime.UtcNow) > x.EndDate).ToListAsync();
                            foreach (var booking in bookings)
                            {
                                var result = await bookingService.CancelExpiredBookingAsync(booking.Id);
                                if(!result.IsSuccess)
                                {
                                    logger.LogError($"Failed to expire booking #{booking.Id}: {string.Join(", ", result.Errors)}");
                                }
                            }

                            await context.Notifications.Where(x => x.CreatedAt < DateTime.UtcNow.AddMonths(-6)).ExecuteDeleteAsync();

                            var removeTimeForRefreshToken = DateTime.UtcNow.AddMonths(-3);
                            await context.RefreshTokens.Where(x => (x.RevokedAt != null && x.RevokedAt < removeTimeForRefreshToken) || x.ExpiresAt < removeTimeForRefreshToken).ExecuteDeleteAsync();
                        }
                        
                    }
                } 
                catch (Exception ex) 
                {
                    logger.LogError(ex, "Booking Lifecycle Backgrounds Service Error!");
                }
            }
        }
    }
}
