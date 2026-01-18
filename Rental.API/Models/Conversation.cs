using Rental.API.Common;
using System.Collections.ObjectModel;

namespace Rental.API.Models
{
    public class Conversation : ISoftDelete
    {
        public int Id { get; set; }
        public string User1Id { get; set; } = string.Empty;
        public User User1 { get; set; }
        public string User2Id { get; set; } = string.Empty;
        public User User2 { get; set; }
        public DateTime LastMessageAt { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        public ICollection<Message> Messages { get; set; }
    }
}
