using Microsoft.EntityFrameworkCore;
using Rental.API.Common;
using Rental.API.Data;
using Rental.API.Models.Responses;

namespace Rental.API.Services
{
    public class NotificationService : INotificationService
    {
        private readonly AppDbContext context;
        public NotificationService(AppDbContext context)
        {
            this.context = context;
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
    }
}
