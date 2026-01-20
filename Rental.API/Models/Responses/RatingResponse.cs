namespace Rental.API.Models.Responses
{
    public class RatingResponse
    {
        public string RaterId { get; set; } = string.Empty;
        public string RaterName { get; set;} = string.Empty;

        public int Rate { get; set; }
        public string? Comment { get; set; }
        public double NewAverageRating { get; set; }
        public int NewRatingCount { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
