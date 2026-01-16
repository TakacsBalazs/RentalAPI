namespace Rental.API.Models.Requests
{
    public class UpdateToolRequest
    {
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public decimal DailyPrice { get; set; }
        public decimal SecurityDeposit { get; set; }
        public string Location { get; set; } = string.Empty;
        public Category Category { get; set; }

        public bool? IsActive { get; set; }

        public DateOnly AvailableUntil { get; set; }
    }
}
