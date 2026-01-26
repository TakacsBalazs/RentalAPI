using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Rental.API.Common;
using Rental.API.Data;
using Rental.API.Hubs;
using Rental.API.Models;
using Rental.API.Models.Responses;

namespace Rental.API.Services
{
    public class NotificationService : INotificationService
    {
        private readonly AppDbContext context;
        private readonly IHubContext<RentalHub, IRentalClient> hubContext;
        public NotificationService(AppDbContext context, IHubContext<RentalHub, IRentalClient> hubContext)
        {
            this.context = context;
            this.hubContext = hubContext;
        }
        public async Task<Result<IEnumerable<NotificationResponse>>> GetUserNotificationsAsync(string userId)
        {
            var response = await context.Notifications.Where(x => x.UserId == userId).OrderByDescending(x => x.CreatedAt).Select(x => new NotificationResponse
            {
                Id = x.Id,
                Title = x.Title,
                Message = x.Message,
                IsRead = x.IsRead,
                CreatedAt = x.CreatedAt,
            }).ToListAsync();
            return Result<IEnumerable<NotificationResponse>>.Success(response);
        }

        public async Task<Result<NotificationResponse>> GetNotificationByIdAsync(int id, string userId)
        {
            var notification = await context.Notifications.FindAsync(id);
            if(notification == null)
            {
                return Result<NotificationResponse>.Failure("Invalid Notification Id!");
            }

            if(notification.UserId != userId)
            {
                return Result<NotificationResponse>.Failure("Can't see this notification!");
            }

            if (!notification.IsRead)
            {
                notification.IsRead = true;
                await context.SaveChangesAsync();
            }

            var response = new NotificationResponse
            {
                Id = notification.Id,
                Title = notification.Title,
                Message = notification.Message,
                IsRead = true,
                CreatedAt = notification.CreatedAt,
            };

            return Result<NotificationResponse>.Success(response);
        }

        public async Task<Result> DeleteNotificationAsync(int id, string userId)
        {
            var notification = await context.Notifications.FindAsync(id);
            if(notification == null) {
                return Result.Failure("Invalid Notification Id!");
            }

            if(notification.UserId != userId)
            {
                return Result.Failure("Can't remove this notification!");
            }

            context.Notifications.Remove(notification);
            await context.SaveChangesAsync();
            return Result.Success();
        }

        public async Task SendNotificationAsync(string userId, string title, string message)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message
            };

            context.Notifications.Add(notification);
            await context.SaveChangesAsync();

            var response = new NotificationResponse
            {
                Id = notification.Id,
                Title = notification.Title,
                Message = notification.Message,
                IsRead = false,
                CreatedAt = notification.CreatedAt,
            };
            await hubContext.Clients.User(userId).ReceiveNotification(response);
        }
    }
}
