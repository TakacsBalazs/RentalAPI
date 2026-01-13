using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Rental.API.Common;
using Rental.API.Extensions;
using Rental.API.Models;
using Rental.API.Models.Requests;

namespace Rental.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> userManager;
        private readonly IConfiguration configuration;
        private readonly IServiceProvider serviceProvider;

        public AuthService(UserManager<User> userManager, IConfiguration configuration, IServiceProvider serviceProvider)
        {
            this.userManager = userManager;
            this.configuration = configuration;
            this.serviceProvider = serviceProvider;
        }

        public async Task<Result> RegisterAsync(CreateUserRequest request)
        {
            var validate = await serviceProvider.ValidateRequestAsync<CreateUserRequest>(request);
            if(!validate.IsSuccess)
            {
                return validate;
            }

            var user = new User
            {
                Email = request.Email,
                UserName = request.UserName,
                FullName = request.FullName,
            };

            var result = await userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(x => x.Description).ToList();
                return Result.Failure(errors);
            }

            return Result.Success();
        }
    }
}
