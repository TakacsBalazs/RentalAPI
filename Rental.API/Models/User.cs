using Microsoft.AspNetCore.Identity;

namespace Rental.API.Models
{
    public class User : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Tool> Tools { get; set; }
    }
}
