using Microsoft.AspNetCore.Identity;
using Rental.API.Common;

namespace Rental.API.Models
{
    public class User : IdentityUser, ISoftDelete
    {
        public string FullName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Tool> Tools { get; set; }

        public ICollection<Booking> Bookings { get; set; }

        public decimal Balance { get; set; } = 0;
        public decimal LockedBalance { get; set; } = 0;

        public ICollection<Transaction> Transactions { get; set; }

        public ICollection<Rating> ReceivedRatings { get; set; }

        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
    }
}
