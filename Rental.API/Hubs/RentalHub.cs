using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Rental.API.Data;

namespace Rental.API.Hubs
{
    [Authorize]
    public class RentalHub : Hub<IRentalClient>
    {
        private readonly AppDbContext context;
        public RentalHub(AppDbContext context)
        {
            this.context = context;
        }

        public async Task MarkMessageAsRead(int messageId)
        {
            var userId = Context.UserIdentifier;
            var message = await context.Messages.Include(x => x.Conversation).FirstOrDefaultAsync(x => x.Id == messageId);
            if(message == null) {
                return;
            }

            if (message.IsRead)
            {
                return;
            }

            if(message.SenderId == userId)
            {
                return;
            }

            if(message.Conversation.User1Id != userId && message.Conversation.User2Id != userId)
            {
                return;
            }

            message.IsRead = true;
            await context.SaveChangesAsync();

            string getterId = message.Conversation.User1Id != userId ? message.Conversation.User1Id : message.Conversation.User2Id;
            await Clients.User(getterId).MessageRead(messageId);
        }
    }
}
