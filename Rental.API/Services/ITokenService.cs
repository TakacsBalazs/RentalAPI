using Rental.API.Models;

namespace Rental.API.Services
{
    public interface ITokenService
    {
        string GenerateJwtToken(User user);

        RefreshToken GenerateRefreshToken(string userId);
    }
}
