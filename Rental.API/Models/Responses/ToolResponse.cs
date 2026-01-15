using Rental.API.Models.Dtos;

namespace Rental.API.Models.Responses
{
    public class ToolResponse
    {

        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal DailyPrice { get; set; }
        public decimal SecurityDeposit { get; set; }
        public string Location { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public Category Category { get; set; }
        public DateOnly AvailableUntil { get; set; }
        public UserDto User { get; set; }
    }
}
