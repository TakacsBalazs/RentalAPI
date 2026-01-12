using Rental.API.Common;

namespace Rental.API
{
    public interface IAuthService
    {
        Task<Result> RegisterAsync(CreateUserRequest request);
    }
}
