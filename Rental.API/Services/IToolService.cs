using Rental.API.Common;
using Rental.API.Models.Requests;
using Rental.API.Models.Responses;

namespace Rental.API.Services
{
    public interface IToolService
    {

        Task<Result<ToolResponse>> CreateToolAsync(CreateToolRequest request, string userId);

        Task<Result<IEnumerable<ToolResponse>>> GetAllToolAsync(GetToolsRequest request);

        Task<Result<ToolResponse>> GetToolByIdAsync(int id);
    }
}
