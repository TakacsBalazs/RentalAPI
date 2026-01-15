namespace Rental.API.Models.Responses
{
    public class ToolUnavailabilityResponse
    {
        public int Id { get; set; }

        public DateOnly StartDate { get; set; }

        public DateOnly EndDate { get; set; }

        public int ToolId { get; set; }
    }
}
