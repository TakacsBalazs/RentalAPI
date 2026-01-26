using Rental.API.Common;
using Rental.API.Models.Responses;

namespace Rental.API.Services
{
    public interface INotificationService
    {
        Task<Result<IEnumerable<NotificationResponse>>> GetUserNotificationsAsync(string userId);
        Task<Result<NotificationResponse>> GetNotificationByIdAsync(int id, string userId);
    }
}
