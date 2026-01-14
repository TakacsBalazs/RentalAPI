using Rental.API.Common;
using Rental.API.Models.Requests;
using Rental.API.Models.Responses;

namespace Rental.API
{
    public interface IAuthService
    {
        Task<Result> RegisterAsync(CreateUserRequest request);

        Task<Result<UserLoginResponse>> LoginAsync(UserLoginRequest request);
    }
}
