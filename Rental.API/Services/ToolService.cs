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
                Location = request.Location,
                AvailableUntil = request.AvailableUntil    
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
                AvailableUntil = tool.AvailableUntil,
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

            if(request.IsActive != null)
            {
                tools = tools.Where(x => x.IsActive == request.IsActive);
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
                AvailableUntil = x.AvailableUntil,
                User = new UserDto
                {
                    Id = x.User.Id,
                    FullName = x.User.FullName
                }
            }).ToListAsync();

            return Result<IEnumerable<ToolResponse>>.Success(response);
        }

        public async Task<Result<ToolResponse>> GetToolByIdAsync(int id, string userId)
        {
            var tool = await context.Tools.Include(x => x.User).FirstOrDefaultAsync(x => x.Id == id);
            if(tool == null)
            {
                return Result<ToolResponse>.Failure("Invalid id!");
            }

            if(tool.UserId != userId && !tool.IsActive)
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
                AvailableUntil = tool.AvailableUntil,
                User = new UserDto
                {
                    Id = tool.User.Id,
                    FullName = tool.User.FullName
                }
            };

            return Result<ToolResponse>.Success(response);
        }

        public async Task<Result<ToolResponse>> UpdateToolAsync(UpdateToolRequest request, int id, string userId)
        {
            var validate = await serviceProvider.ValidateRequestAsync<UpdateToolRequest>(request);
            if (!validate.IsSuccess)
            {
                return Result<ToolResponse>.Failure(validate.Errors);
            }

            var tool = await context.Tools.Include(x => x.User).FirstOrDefaultAsync(x => x.Id == id);
            if (tool == null)
            {
                return Result<ToolResponse>.Failure("Invalid id!");
            }

            if(tool.UserId != userId)
            {
                return Result<ToolResponse>.Failure("You are not the owner of this tool!");
            }

            tool.Name = request.Name;
            tool.IsActive = request.IsActive!.Value;
            tool.Description = request.Description;
            tool.DailyPrice = request.DailyPrice;
            tool.SecurityDeposit = request.SecurityDeposit;
            tool.AvailableUntil = request.AvailableUntil;
            tool.Category = request.Category;
            tool.Location = request.Location;

            await context.SaveChangesAsync();

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
                AvailableUntil = tool.AvailableUntil,
                User = new UserDto
                {
                    Id = tool.User.Id,
                    FullName = tool.User.FullName
                }
            };

            return Result<ToolResponse>.Success(response);
        }

        public async Task<Result<IEnumerable<CalendarResponse>>> GetToolCalendarAsync(int toolId)
        {
            bool exists = await context.Tools.AnyAsync(x => x.Id == toolId && x.IsActive);
            if (!exists)
            {
                return Result<IEnumerable<CalendarResponse>>.Failure("Invalid Tool Id!");
            }

            var bookings = await context.Bookings.Where(x => x.ToolId == toolId && x.Status != BookingStatus.CancelledByRenter && x.Status != BookingStatus.CancelledByOwner).Select(x => new CalendarResponse
            {
                StartDate = x.StartDate, 
                EndDate = x.EndDate
            }).ToListAsync();

            var unavailabilities = await context.ToolUnavailabilities.Where(x => x.ToolId == toolId).Select(x => new CalendarResponse
            {
                StartDate = x.StartDate,
                EndDate = x.EndDate
            }).ToListAsync();

            var calendar = bookings.Concat(unavailabilities).OrderBy(x => x.StartDate).ToList();
            return Result<IEnumerable<CalendarResponse>>.Success(calendar);
        }
    }
}
