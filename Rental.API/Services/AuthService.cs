using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Rental.API.Common;
using Rental.API.Extensions;
using Rental.API.Models;
using Rental.API.Models.Requests;
using Rental.API.Models.Responses;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

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

        public async Task<Result<UserLoginResponse>> LoginAsync(UserLoginRequest request)
        {
            var validate = await serviceProvider.ValidateRequestAsync<UserLoginRequest>(request);
            if(!validate.IsSuccess)
            {
                return Result<UserLoginResponse>.Failure(validate.Errors);
            }

            User? user = null;
            if (request.Identifier.Contains('@'))
            {
                user = await userManager.FindByEmailAsync(request.Identifier);
            } else
            {
                user = await userManager.FindByNameAsync(request.Identifier);
            }

            if(user == null)
            {
                return Result<UserLoginResponse>.Failure("Invalid email or username!");
            }

            if (await userManager.IsLockedOutAsync(user))
            {
                return Result<UserLoginResponse>.Failure("The account is temporarily locked.");
            }

            var passwordCheck = await userManager.CheckPasswordAsync(user, request.Password);
            if (!passwordCheck)
            {
                await userManager.AccessFailedAsync(user);

                if(await userManager.IsLockedOutAsync(user))
                {
                    return Result<UserLoginResponse>.Failure("The account is temporarily locked.");
                }

                return Result<UserLoginResponse>.Failure("Invalid password!");
            }

            await userManager.ResetAccessFailedCountAsync(user);

            var tokenResult = GenerateJwtToken(user);
            return Result<UserLoginResponse>.Success(tokenResult);
        }

        private UserLoginResponse GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:SecurityKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(10),
                signingCredentials: creds
            );

            return new UserLoginResponse
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = token.ValidTo
            };
        }
    }
}
