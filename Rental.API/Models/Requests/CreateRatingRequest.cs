namespace Rental.API.Models.Requests
{
    public class CreateRatingRequest
    {
        public string RatedUserId { get; set; } = string.Empty;
        public int Rate { get; set; }
        public string? Comment { get; set; }
    }
}
