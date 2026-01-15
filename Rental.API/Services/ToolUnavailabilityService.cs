using Microsoft.EntityFrameworkCore;
using Rental.API.Common;
using Rental.API.Data;
using Rental.API.Extensions;
using Rental.API.Models;
using Rental.API.Models.Requests;
using Rental.API.Models.Responses;
using System.Collections.Generic;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Rental.API.Services
{
    public class ToolUnavailabilityService : IToolUnavailabilityService
    {
        private readonly AppDbContext context;
        private readonly IServiceProvider serviceProvider;

        public ToolUnavailabilityService(AppDbContext context, IServiceProvider serviceProvider)
        {
            this.context = context;
            this.serviceProvider = serviceProvider;
        }

        public async Task<Result<ToolUnavailabilityResponse>> CreateToolUnavailabilityAsync(CreateToolUnavailabilityRequest request, string userId)
        {
            var validate = await serviceProvider.ValidateRequestAsync<CreateToolUnavailabilityRequest>(request);
            if (!validate.IsSuccess)
            {
                return Result<ToolUnavailabilityResponse>.Failure(validate.Errors);
            }

            var tool = await context.Tools.FindAsync(request.ToolId);
            if(tool == null)
            {
                return Result<ToolUnavailabilityResponse>.Failure("Invalid Tool Id!");
            }

            if(tool.UserId != userId)
            {
                return Result<ToolUnavailabilityResponse>.Failure("You don't have this tool!");
            }

            if(request.StartDate > tool.AvailableUntil || request.EndDate > tool.AvailableUntil)
            {
                return Result<ToolUnavailabilityResponse>.Failure("You have to change availability of the tool!");
            }

            bool isOverlapping = await context.ToolUnavailabilities.AnyAsync(x => x.ToolId == request.ToolId && x.StartDate <= request.EndDate && x.EndDate >= request.StartDate);
            if(isOverlapping)
            {
                return Result<ToolUnavailabilityResponse>.Failure("You have already selected this period!");
            }

            var toolUnavailability = new ToolUnavailability
            {
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                ToolId = request.ToolId,
            };
            context.ToolUnavailabilities.Add(toolUnavailability);
            await context.SaveChangesAsync();

            var response = new ToolUnavailabilityResponse
            {
                Id = toolUnavailability.Id,
                StartDate = toolUnavailability.StartDate,
                EndDate = toolUnavailability.EndDate,
                ToolId = toolUnavailability.ToolId,
            };
            return Result<ToolUnavailabilityResponse>.Success(response);
        }

        public async Task<Result> DeleteToolUnavailabilityAsync(int toolUnavailabilityId, string userId)
        {
            var toolUnavailability = await context.ToolUnavailabilities.Include(x => x.Tool).FirstOrDefaultAsync(x => x.Id == toolUnavailabilityId);
            if (toolUnavailability == null)
            {
                return Result.Failure("Invalid Tool Unavailability Id");
            }

            if(toolUnavailability.Tool.UserId != userId) {
                return Result.Failure("You can't delete this!");
            }

            context.ToolUnavailabilities.Remove(toolUnavailability);
            await context.SaveChangesAsync();

            return Result.Success();
        }

        public async Task<Result<IEnumerable<ToolUnavailabilityResponse>>> GetToolUnavailabilitiesAsync(GetToolUnavailabilitiesRequest request)
        {
            var validate = await serviceProvider.ValidateRequestAsync<GetToolUnavailabilitiesRequest>(request);
            if (!validate.IsSuccess)
            {
                return Result<IEnumerable<ToolUnavailabilityResponse>>.Failure(validate.Errors);
            }

            var toolUnavailibilities = context.ToolUnavailabilities
                .Where(x => x.ToolId == request.ToolId);

            if (request.From != null)
            {
                toolUnavailibilities = toolUnavailibilities.Where(x => x.EndDate >= request.From);
            }

            if (request.To != null)
            {
                toolUnavailibilities = toolUnavailibilities.Where(x => x.StartDate <= request.To);
            }

            var response = await toolUnavailibilities.OrderBy(x => x.StartDate)
                .Select(x => new ToolUnavailabilityResponse
            {
                Id = x.Id,
                ToolId = x.ToolId,
                StartDate = x.StartDate,
                EndDate = x.EndDate
            }).ToListAsync();

            return Result<IEnumerable<ToolUnavailabilityResponse>>.Success(response);
        }
    }
}
