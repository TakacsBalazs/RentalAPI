using Rental.API.Common;
using Rental.API.Models.Requests;
using Rental.API.Models.Responses;

namespace Rental.API.Services
{
    public interface IToolService
    {

        Task<Result<CreateToolResponse>> CreateToolAsync(CreateToolRequest request, string userId);
    }
}
