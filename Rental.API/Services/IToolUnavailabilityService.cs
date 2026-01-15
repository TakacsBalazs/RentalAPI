using Rental.API.Common;
using Rental.API.Models.Requests;
using Rental.API.Models.Responses;

namespace Rental.API.Services
{
    public interface IToolUnavailabilityService
    {

        Task<Result<ToolUnavailabilityResponse>> CreateToolUnavailabilityAsync(CreateToolUnavailabilityRequest request, string userId);

        Task<Result> DeleteToolUnavailabilityAsync(int toolUnavailabilityId, string userId);
    }
}
