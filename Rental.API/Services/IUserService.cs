using Rental.API.Common;
using Rental.API.Models.Requests;
using Rental.API.Models.Responses;

namespace Rental.API.Services
{
    public interface IUserService
    {
        Task<Result<MyProfileResponse>> GetOwnProfileAsync(string userId);
        Task<Result<PublicUserResponse>> GetUserByIdAsync(string userId);
        Task<Result<MyProfileResponse>> UpdateOwnProfileAsync(string userId, UpdateUserRequest request);
    }
}
