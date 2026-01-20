using Rental.API.Common;
using Rental.API.Models.Requests;
using Rental.API.Models.Responses;

namespace Rental.API.Services
{
    public interface IRatingService
    {
        Task<Result<RatingResponse>> CreateRatingAsync(CreateRatingRequest request, string raterId);

    }
}
