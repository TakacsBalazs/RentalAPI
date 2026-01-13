using System.ComponentModel.DataAnnotations;

namespace Rental.API.Models.Requests
{
    public class CreateUserRequest
    {
        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string ConfirmPassword { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;
    }
}
