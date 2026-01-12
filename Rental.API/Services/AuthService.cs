using Microsoft.AspNetCore.Identity;
using Rental.API.Common;
using Rental.API.Models;
using Rental.API.Models.Requests;

namespace Rental.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> userManager;
        private readonly IConfiguration configuration;

        public AuthService(UserManager<User> userManager, IConfiguration configuration)
        {
            this.userManager = userManager;
            this.configuration = configuration;
        }

        public Task<Result> RegisterAsync(CreateUserRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
