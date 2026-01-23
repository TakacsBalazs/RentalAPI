namespace Rental.API.Models.Responses
{
    public class MyProfileResponse
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public decimal Balance { get; set; }
        public decimal LockedBalance { get; set; }

    }
}
