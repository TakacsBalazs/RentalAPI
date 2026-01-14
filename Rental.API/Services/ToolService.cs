using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Rental.API.Common;
using Rental.API.Data;
using Rental.API.Extensions;
using Rental.API.Models;
using Rental.API.Models.Dtos;
using Rental.API.Models.Requests;
using Rental.API.Models.Responses;
using System.Threading.Tasks;

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

        public async Task<Result<ToolResponse>> CreateToolAsync(CreateToolRequest request, string userId)
        {
            var validate = await serviceProvider.ValidateRequestAsync<CreateToolRequest>(request);
            if (!validate.IsSuccess)
            {
                return Result<ToolResponse>.Failure(validate.Errors);
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

            var user = await context.Users.FindAsync(userId);

            var response = new ToolResponse
            {
                Id = tool.Id,
                Name = tool.Name,
                IsActive = tool.IsActive,
                Description = tool.Description,
                Category = tool.Category,
                Location = tool.Location,
                DailyPrice = tool.DailyPrice,
                SecurityDeposit = tool.SecurityDeposit,
                User = new UserDto
                {
                    Id = userId,
                    FullName = user?.FullName ?? "Unkown User"
                }
            };

            return Result<ToolResponse>.Success(response);
        }

        public async Task<Result<IEnumerable<ToolResponse>>> GetAllToolAsync(GetToolsRequest request)
        {
            var tools = context.Tools.AsQueryable();

            if (!string.IsNullOrEmpty(request.UserId))
            {
                tools = tools.Where(x => x.UserId == request.UserId);
            }   
                    
            var response = await tools.Select(x => new ToolResponse
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

            return Result<IEnumerable<ToolResponse>>.Success(response);
        }

        public async Task<Result<ToolResponse>> GetToolByIdAsync(int id)
        {
            var tool = await context.Tools.Include(x => x.User).FirstOrDefaultAsync(x => x.Id == id);
            if(tool == null)
            {
                return Result<ToolResponse>.Failure("Invalid id!");
            }
            var response = new ToolResponse
            {
                Id = tool.Id,
                Name = tool.Name,
                IsActive = tool.IsActive,
                Description = tool.Description,
                Category = tool.Category,
                Location = tool.Location,
                DailyPrice = tool.DailyPrice,
                SecurityDeposit = tool.SecurityDeposit,
                User = new UserDto
                {
                    Id = tool.User.Id,
                    FullName = tool.User.FullName
                }
            };

            return Result<ToolResponse>.Success(response);
        }
    }
}
