using Rental.API.Common;

namespace Rental.API.Models
{
    public class Tool : ISoftDelete
    {

        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;


        public decimal DailyPrice { get; set; }
        public decimal SecurityDeposit { get; set; }

        public string Location { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public string UserId { get; set; } = string.Empty;
        public User User { get; set; }

        public Category Category { get; set; }

        public DateOnly AvailableUntil { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
    }

    public enum Category
    {
        Construction,
        Gardening,
        Cleaning,
        Woodworking
    }
}
