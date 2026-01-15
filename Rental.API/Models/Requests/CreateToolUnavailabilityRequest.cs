namespace Rental.API.Models.Requests
{
    public class CreateToolUnavailabilityRequest
    {
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public int ToolId { get; set; }
    }
}
