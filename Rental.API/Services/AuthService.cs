using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Rental.API.Common;
using Rental.API.Data;
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
        private readonly ITokenService tokenService;
        private readonly IServiceProvider serviceProvider;
        private readonly AppDbContext context;

        public AuthService(UserManager<User> userManager, ITokenService tokenService, IServiceProvider serviceProvider, AppDbContext context)
        {
            this.userManager = userManager;
            this.tokenService = tokenService;
            this.serviceProvider = serviceProvider;
            this.context = context;
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

            var accessToken = tokenService.GenerateJwtToken(user);

            var refreshToken = tokenService.GenerateRefreshToken(user.Id);

            var now = DateTime.UtcNow;
            int maxSessions = 6;
            var activeTokens = await context.RefreshTokens.Where(x => x.UserId == user.Id && x.RevokedAt == null && x.ExpiresAt >  now).OrderBy(x => x.CreatedAt).ToListAsync();
            if(activeTokens.Count >= maxSessions)
            {
                var tokensToRevoke = activeTokens.Take(activeTokens.Count - maxSessions + 1).ToList();

                foreach (var token in tokensToRevoke)
                {

                    token.RevokedAt = now;
                    token.ReplacedByToken = refreshToken.Token;
 
                }
            }

            context.RefreshTokens.Add(refreshToken);
            await context.SaveChangesAsync();

            var response = new UserLoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token
            };
            return Result<UserLoginResponse>.Success(response);
        }

        public async Task<Result<UserLoginResponse>> RefreshTokenAsync(RefreshTokenRequest request)
        {
            var validate = await serviceProvider.ValidateRequestAsync<RefreshTokenRequest>(request);
            if (!validate.IsSuccess)
            {
                return Result<UserLoginResponse>.Failure(validate.Errors);
            }

            var storedToken = await context.RefreshTokens.Include(x => x.User).FirstOrDefaultAsync(x => x.Token == request.RefreshToken);
            if (storedToken == null)
            {
                return Result<UserLoginResponse>.Failure("Invalid Refresh Token!");
            }

            if(storedToken.ExpiresAt < DateTime.UtcNow)
            {
                return Result<UserLoginResponse>.Failure("Expired Refresh Token!");
            }

            if(storedToken.RevokedAt != null)
            {
                return Result<UserLoginResponse>.Failure("Used Refresh Token!");
            }

            storedToken.RevokedAt = DateTime.UtcNow;

            var accessToken = tokenService.GenerateJwtToken(storedToken.User);

            var refreshToken = tokenService.GenerateRefreshToken(storedToken.User.Id);

            storedToken.ReplacedByToken = refreshToken.Token;

            context.RefreshTokens.Add(refreshToken);

            await context.SaveChangesAsync();

            var response = new UserLoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token
            };
            return Result<UserLoginResponse>.Success(response);
        }
    }
}
