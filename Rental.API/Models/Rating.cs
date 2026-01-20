namespace Rental.API.Models
{
    public class Rating
    {
        public string RatedUserId { get; set; }
        public User RatedUser { get; set; }
        public string RaterUserId { get; set; }
        public User RaterUser { get; set; }
        public int Rate { get; set; }
        public string? Comment { get; set;}
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
