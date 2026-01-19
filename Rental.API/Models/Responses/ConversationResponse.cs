using Rental.API.Models.Dtos;

namespace Rental.API.Models.Responses
{
    public class ConversationResponse
    {
        public int Id { get; set; }
        public ChatPartnerDto? Partner { get; set; }

        public string? LastMessagePreview { get; set; }
        public DateTime LastMessageAt { get; set; }
        public bool HasUnreadMessages { get; set; }
    }
}
