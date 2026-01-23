using Microsoft.EntityFrameworkCore;
using Rental.API.Common;
using Rental.API.Data;
using Rental.API.Extensions;
using Rental.API.Models.Requests;
using Rental.API.Models.Responses;

namespace Rental.API.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext context;
        private readonly IServiceProvider serviceProvider;
        public UserService(AppDbContext context , IServiceProvider serviceProvider)
        {
            this.context = context;
            this.serviceProvider = serviceProvider;
        }

        public async Task<Result<MyProfileResponse>> GetOwnProfileAsync(string userId)
        {
            var user = await context.Users.FindAsync(userId);
            if(user == null)
            {
                return Result<MyProfileResponse>.Failure("Invalid User Id!");
            }

            var response = new MyProfileResponse
            {
                Id = user.Id,
                FullName = user.FullName,
                Username = user.UserName!,
                CreatedAt = user.CreatedAt,
                Email = user.Email!,
                Balance = user.Balance,
                LockedBalance = user.LockedBalance,
            };

            return Result<MyProfileResponse>.Success(response);
        }

        public async Task<Result<PublicUserResponse>> GetUserByIdAsync(string userId)
        {
            var user = await context.Users.Include(x => x.ReceivedRatings).FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
            {
                return Result<PublicUserResponse>.Failure("Invalid User Id!");
            }

            int count = user.ReceivedRatings.Count();
            double rate = count > 0 ? user.ReceivedRatings.Average(x => x.Rate) : 0;

            var response = new PublicUserResponse
            {
                Id = user.Id,
                FullName = user.FullName,
                CreatedAt = user.CreatedAt,
                RatersCount = user.ReceivedRatings.Count(),
                Rate = rate
            };
            return Result<PublicUserResponse>.Success(response);
        }

        public async Task<Result<MyProfileResponse>> UpdateOwnProfileAsync(string userId, UpdateUserRequest request)
        {
            var validate = await serviceProvider.ValidateRequestAsync<UpdateUserRequest>(request);
            if(!validate.IsSuccess)
            {
                return Result<MyProfileResponse>.Failure(validate.Errors);
            }

            var user = await context.Users.FindAsync(userId);
            if (user == null)
            {
                return Result<MyProfileResponse>.Failure("Invalid User Id!");
            }

            user.FullName = request.FullName;
            await context.SaveChangesAsync();

            var response = new MyProfileResponse
            {
                Id = user.Id,
                FullName = user.FullName,
                Username = user.UserName!,
                CreatedAt = user.CreatedAt,
                Email = user.Email!,
                Balance = user.Balance,
                LockedBalance = user.LockedBalance,
            };

            return Result<MyProfileResponse>.Success(response);
        }
    }
}
