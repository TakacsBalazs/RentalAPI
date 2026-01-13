using Rental.API.Common;
using Rental.API.Models.Requests;

namespace Rental.API
{
    public interface IAuthService
    {
        Task<Result> RegisterAsync(CreateUserRequest request);
    }
}
