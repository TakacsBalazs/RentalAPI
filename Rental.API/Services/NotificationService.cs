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
    }
}
