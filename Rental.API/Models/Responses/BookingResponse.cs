using Rental.API.Models.Dtos;

namespace Rental.API.Models.Responses
{
    public class BookingResponse
    {
        public int Id { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public BookingStatus Status { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal SecurityDeposit { get; set; }
        public DateTime CreatedAt { get; set; }
        public ToolDto Tool { get; set; }
        public UserDto? Renter { get; set; }
    }
}
