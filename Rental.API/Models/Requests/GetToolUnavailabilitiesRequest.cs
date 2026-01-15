namespace Rental.API.Models.Requests
{
    public class GetToolUnavailabilitiesRequest
    {
        public int ToolId { get; set; }

        public DateOnly? From { get; set; }

        public DateOnly? To { get; set; }
    }
}
