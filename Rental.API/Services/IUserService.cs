using Rental.API.Common;
using Rental.API.Models.Responses;

namespace Rental.API.Services
{
    public interface IUserService
    {
        Task<Result<MyProfileResponse>> GetOwnProfileAsync(string userId);
    }
}
