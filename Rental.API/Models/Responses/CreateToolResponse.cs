namespace Rental.API.Models.Responses
{
    public class CreateToolResponse
    {

        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal DailyPrice { get; set; }
        public decimal SecurityDeposit { get; set; }
        public string Location { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string UserId { get; set; } = string.Empty;
        public Category Category { get; set; }

    }
}
