using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Rental.API.Common;
using Rental.API.Data;
using Rental.API.Extensions;
using Rental.API.Models;
using Rental.API.Models.Dtos;
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
                IsActive = tool.IsActive,
                Description = tool.Description,
                Category = tool.Category,
                Location = tool.Location,
                DailyPrice = tool.DailyPrice,
                SecurityDeposit = tool.SecurityDeposit
            };

            return Result<CreateToolResponse>.Success(response);
        }

        public async Task<Result<IEnumerable<ToolResponse>>> GetAllToolAsync()
        {
            var tools = await context.Tools.Include(x => x.User).Select(x => new ToolResponse
            {
                Id = x.Id,
                Name = x.Name,
                IsActive = x.IsActive,
                Description = x.Description,
                Category = x.Category,
                Location = x.Location,
                DailyPrice = x.DailyPrice,
                SecurityDeposit = x.SecurityDeposit,
                User = new UserDto
                {
                    Id = x.User.Id,
                    FullName = x.User.FullName
                }
            }).ToListAsync();

            return Result<IEnumerable<ToolResponse>>.Success(tools);
        }
    }
}
