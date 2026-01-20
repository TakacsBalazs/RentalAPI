using Microsoft.EntityFrameworkCore;
using Rental.API.Common;
using Rental.API.Data;
using Rental.API.Extensions;
using Rental.API.Models;
using Rental.API.Models.Requests;
using Rental.API.Models.Responses;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Rental.API.Services
{
    public class RatingService : IRatingService
    {
        private readonly AppDbContext context;
        private readonly IServiceProvider serviceProvider;
        public RatingService(AppDbContext context, IServiceProvider serviceProvider)
        {
            this.context = context;
            this.serviceProvider = serviceProvider;
        }
        public async Task<Result<RatingResponse>> CreateRatingAsync(CreateRatingRequest request, string raterId)
        {
            var validate = await serviceProvider.ValidateRequestAsync<CreateRatingRequest>(request);
            if (!validate.IsSuccess)
            {
                return Result<RatingResponse>.Failure(validate.Errors);
            }

            if (request.RatedUserId == raterId)
            {
                return Result<RatingResponse>.Failure("Can't rate yourself");
            }

            var rating = await context.Ratings.Include(x => x.RaterUser).FirstOrDefaultAsync(x => x.RatedUserId == request.RatedUserId && x.RaterUserId == raterId);
            if (rating == null)
            {
                rating = new Rating
                {
                    RatedUserId = request.RatedUserId,
                    RaterUserId = raterId,
                    Rate = request.Rate,
                    Comment = request.Comment
                };
                context.Ratings.Add(rating);
            } else
            {
                rating.Rate = request.Rate;
                rating.Comment = request.Comment;
                rating.UpdatedAt = DateTime.UtcNow;
            }

            await context.SaveChangesAsync();

            var stats = await context.Ratings.Where(x => x.RatedUserId == request.RatedUserId).GroupBy(x => x.RatedUserId)
                .Select(x => new
                {
                    Average = x.Average(y => y.Rate),
                    Count = x.Count()
                }).FirstAsync();

            string? raterName = rating.RaterUser?.FullName;
            if (raterName == null)
            {
                var user = await context.Users.FindAsync(raterId);
                raterName = user?.FullName ?? "Unknown";
            }

            var response = new RatingResponse
            {
                RaterId = rating.RaterUserId,
                RaterName = raterName,
                Rate = rating.Rate,
                Comment = rating.Comment,
                UpdatedAt = rating.UpdatedAt,
                CreatedAt = rating.CreatedAt,
                NewAverageRating = stats.Average,
                NewRatingCount = stats.Count
            };
            return Result<RatingResponse>.Success(response);
        }
    }
}
