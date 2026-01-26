using Azure;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Rental.API.Common;
using Rental.API.Data;
using Rental.API.Extensions;
using Rental.API.Hubs;
using Rental.API.Models;
using Rental.API.Models.Dtos;
using Rental.API.Models.Requests;
using Rental.API.Models.Responses;

namespace Rental.API.Services
{
    public class ChatService : IChatService
    {
        private readonly AppDbContext context;
        private readonly IServiceProvider serviceProvider;
        private readonly IHubContext<RentalHub, IRentalClient> hubContext;
        public ChatService(AppDbContext context, IServiceProvider serviceProvider, IHubContext<RentalHub, IRentalClient> hubContext)
        {
            this.context = context;
            this.serviceProvider = serviceProvider;
            this.hubContext = hubContext;
        }
        public async Task<Result<ConversationResponse>> CreateConversationAsync(CreateConversationRequest request, string senderId)
        {
            var validate = await serviceProvider.ValidateRequestAsync<CreateConversationRequest>(request);
            if (!validate.IsSuccess)
            {
                return Result<ConversationResponse>.Failure(validate.Errors);
            }

            if (senderId == request.TargetUserId)
            {
                return Result<ConversationResponse>.Failure("Can't start a conversation with yourself.");
            }

            var partner = await context.Users.FindAsync(request.TargetUserId);
            if (partner == null)
            {
                return Result<ConversationResponse>.Failure("Invalid Target User!");
            }

            string user1Id = string.Compare(senderId, request.TargetUserId) < 0 ? senderId : request.TargetUserId;
            string user2Id = string.Compare(senderId, request.TargetUserId) < 0 ? request.TargetUserId : senderId;

            var conversation = await context.Conversations.IgnoreQueryFilters().Include(x => x.Messages).FirstOrDefaultAsync(x => x.User1Id == user1Id && x.User2Id == user2Id);
            ConversationResponse? response = null;
            if (conversation == null)
            {
                conversation = new Conversation
                {
                    User1Id = user1Id,
                    User2Id = user2Id,
                    LastMessageAt = DateTime.UtcNow
                };
                context.Conversations.Add(conversation);
                await context.SaveChangesAsync();

                response = new ConversationResponse { 
                    Id = conversation.Id,
                    Partner = new ChatPartnerDto
                    {
                        Id = partner.Id,
                        FullName = partner.FullName
                    },
                    LastMessagePreview = null,
                    LastMessageAt = conversation.LastMessageAt,
                    HasUnreadMessages = false
                };

                return Result<ConversationResponse>.Success(response);
            } else if(conversation.IsDeleted)
            {
                conversation.IsDeleted = false;
                conversation.DeletedAt = null;
                await context.SaveChangesAsync();
            }
            var messages = conversation.Messages ?? new List<Message>();

            bool hasUnreadMessage = messages.Any(x => x.SenderId != senderId && !x.IsRead);
            string? lastMessage = messages.OrderByDescending(x => x.CreatedAt).FirstOrDefault()?.Content;
            response = new ConversationResponse
            {
                Id = conversation.Id,
                Partner = new ChatPartnerDto
                {
                    Id = partner.Id,
                    FullName = partner.FullName
                },
                LastMessagePreview = lastMessage,
                LastMessageAt = conversation.LastMessageAt,
                HasUnreadMessages = hasUnreadMessage
            };
            return Result<ConversationResponse>.Success(response);
        }

        public async Task<Result<IEnumerable<ConversationResponse>>> GetUserConversationsAsync(string senderId)
        {
            var conversation = await context.Conversations.Include(x => x.Messages).Include(x => x.User1).Include(x => x.User2)
                .Where(x => x.User1Id == senderId || x.User2Id == senderId).OrderByDescending(x => x.LastMessageAt).ToListAsync();

            var response = conversation.Select(x =>
            {
                var partnerUser = (x.User1Id == senderId) ? x.User2 : x.User1;

                var messages = x.Messages ?? new List<Message>();

                bool hasUnreadMessage = messages.Any(x => x.SenderId != senderId && !x.IsRead);
                string? lastMessage = messages.OrderByDescending(x => x.CreatedAt).FirstOrDefault()?.Content;

                return new ConversationResponse
                {
                    Id = x.Id,
                    Partner = partnerUser == null ? null : new ChatPartnerDto
                    {
                        Id = partnerUser.Id,
                        FullName = partnerUser.FullName
                    },
                    LastMessagePreview = lastMessage,
                    LastMessageAt = x.LastMessageAt,
                    HasUnreadMessages = hasUnreadMessage
                };
            });
            return Result<IEnumerable<ConversationResponse>>.Success(response);
        }

        public async Task<Result<MessageResponse>> CreateMessageAsync(CreateMessageRequest request, int conversationId, string senderId)
        {
            var validate = await serviceProvider.ValidateRequestAsync<CreateMessageRequest>(request);
            if (!validate.IsSuccess)
            {
                return Result<MessageResponse>.Failure(validate.Errors);
            }

            var conversation = await context.Conversations.FindAsync(conversationId);
            if(conversation == null)
            {
                return Result<MessageResponse>.Failure("Invalid Conversation Id!");
            }
            if(conversation.User1Id != senderId && conversation.User2Id != senderId)
            {
                return Result<MessageResponse>.Failure("Can't send message to this conversation!");
            }

            var message = new Message
            {
                ConversationId = conversationId,
                Content = request.Text,
                SenderId = senderId
            };
            context.Messages.Add(message);
            conversation.LastMessageAt = DateTime.UtcNow;
            await context.SaveChangesAsync();

            var user = await context.Users.FindAsync(senderId);

            var response = new MessageResponse
            {
                Id = message.Id,
                SenderId = senderId,
                SenderName = user?.FullName ?? "Unknown",
                Content = message.Content,
                CreatedAt = message.CreatedAt,
                IsRead = message.IsRead
            };

            string getterId = conversation.User1Id != senderId ? conversation.User1Id : conversation.User2Id;
            await hubContext.Clients.User(getterId).ReceiveMessage(response);

            return Result<MessageResponse>.Success(response);
        }

        public async Task<Result<IEnumerable<MessageResponse>>> GetConversationMessagesAsync(int conversationId, string userId)
        {
            var conversation = await context.Conversations.Include(x => x.Messages).Include(x => x.User1)
                .Include(x => x.User2).FirstOrDefaultAsync(x => x.Id == conversationId);
            if(conversation == null)
            {
                return Result<IEnumerable<MessageResponse>>.Failure("Invalid Conversation Id!");
            }

            if (conversation.User1Id != userId && conversation.User2Id != userId)
            {
                return Result<IEnumerable<MessageResponse>>.Failure("Can't get messages from this conversation!");
            }

            await context.Messages.Where(x => x.SenderId != userId && x.ConversationId == conversationId && x.IsRead == false)
                .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.IsRead, true));
            var response = conversation.Messages.Select(x =>
            {
                var sender = x.SenderId == conversation.User1Id ? conversation.User1 : conversation.User2;
                return new MessageResponse
                {
                    Id = x.Id,
                    SenderId = x.SenderId,
                    SenderName = sender?.FullName ?? "Unknown",
                    Content = x.Content,
                    CreatedAt = x.CreatedAt,
                    IsRead = x.SenderId != userId ? true : x.IsRead
                };
            }).OrderBy(x => x.CreatedAt);
            return Result<IEnumerable<MessageResponse>>.Success(response);
        }
    }
}
