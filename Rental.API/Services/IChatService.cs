using Rental.API.Common;
using Rental.API.Models.Requests;
using Rental.API.Models.Responses;

namespace Rental.API.Services
{
    public interface IChatService
    {
        Task<Result<ConversationResponse>> CreateConversationAsync(CreateConversationRequest request, string senderId);
        Task<Result<IEnumerable<ConversationResponse>>> GetUserConversationsAsync(string senderId);

        Task<Result<MessageResponse>> CreateMessageAsync(CreateMessageRequest request,int conversationId, string senderId);

        Task<Result<IEnumerable<MessageResponse>>> GetConversationMessagesAsync(int conversationId, string userId);
    }
}
