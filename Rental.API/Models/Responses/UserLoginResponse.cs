namespace Rental.API.Models.Responses
{
    public class UserLoginResponse
    {
        public string Token { get; set; } = string.Empty;

        public DateTime Expiration { get; set; }
    }
}
