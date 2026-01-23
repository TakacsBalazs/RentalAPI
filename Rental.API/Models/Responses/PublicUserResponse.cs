namespace Rental.API.Models.Responses
{
    public class PublicUserResponse
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public double Rate { get; set; }
        public int RatersCount { get; set; }
    }
}
