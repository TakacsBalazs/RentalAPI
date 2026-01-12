using Microsoft.AspNetCore.Identity;

namespace Rental.API.Models
{
    public class User : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
    }
}
