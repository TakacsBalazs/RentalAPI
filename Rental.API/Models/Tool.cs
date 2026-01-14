namespace Rental.API.Models
{
    public class Tool
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
    }

    public enum Category
    {
        CONSTRUCTION,
        GARDENING,
        CLEANING,
        WOODWORKING
    }
}
