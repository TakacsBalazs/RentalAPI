using Rental.API.Models.Responses;

namespace Rental.API.Hubs
{
    public interface IRentalClient
    {
        Task ReceiveMessage(MessageResponse message);
        Task MessageRead(int messageId);

        Task ReceiveNotification(NotificationResponse notification);
    }
}
