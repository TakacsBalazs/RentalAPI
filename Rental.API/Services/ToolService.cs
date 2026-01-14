using Microsoft.AspNetCore.Identity;
using Rental.API.Common;
using Rental.API.Data;
using Rental.API.Extensions;
using Rental.API.Models;
using Rental.API.Models.Requests;
using Rental.API.Models.Responses;

namespace Rental.API.Services
{
    public class ToolService : IToolService
    {
        private readonly AppDbContext context;
        private readonly IServiceProvider serviceProvider;

        public ToolService(AppDbContext context, IServiceProvider serviceProvider)
        {
            this.context = context;
            this.serviceProvider = serviceProvider;
        }

        public async Task<Result<CreateToolResponse>> CreateToolAsync(CreateToolRequest request, string userId)
        {
            var validate = await serviceProvider.ValidateRequestAsync<CreateToolRequest>(request);
            if (!validate.IsSuccess)
            {
                return Result<CreateToolResponse>.Failure(validate.Errors);
            }

            var tool = new Tool
            {
                Name = request.Name,
                DailyPrice = request.DailyPrice,
                UserId = userId,
                SecurityDeposit = request.SecurityDeposit,
                Description = request.Description,
                Category = request.Category,
                Location = request.Location
            };

            context.Tools.Add(tool);
            await context.SaveChangesAsync();

            var response = new CreateToolResponse
            {
                Id = tool.Id,
                Name = tool.Name,
                UserId = tool.UserId,
                IsActive = tool.IsActive,
                Description = tool.Description,
                Category = tool.Category,
                Location = tool.Location,
                DailyPrice = tool.DailyPrice,
                SecurityDeposit = tool.SecurityDeposit
            };

            return Result<CreateToolResponse>.Success(response);
        }
    }
}
