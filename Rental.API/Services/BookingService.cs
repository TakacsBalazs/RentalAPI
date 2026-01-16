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
    public class BookingService : IBookingService
    {
        private readonly AppDbContext context;
        private readonly IServiceProvider serviceProvider;

        public BookingService(AppDbContext context, IServiceProvider serviceProvider)
        {
            this.context = context;
            this.serviceProvider = serviceProvider;
        }

        public async Task<Result<BookingResponse>> CreateBookingAsync(CreateBookingRequest request, string renterId)
        {
            var validate = await serviceProvider.ValidateRequestAsync<CreateBookingRequest>(request);
            if (!validate.IsSuccess)
            {
                return Result<BookingResponse>.Failure(validate.Errors);
            }

            var tool = await context.Tools.FirstOrDefaultAsync(x => x.Id == request.ToolId);
            if (tool == null)
            {
                return Result<BookingResponse>.Failure("Invalid Tool Id!");
            }

            if(!tool.IsActive)
            {
                return Result<BookingResponse>.Failure("Invalid Tool Id!");
            }

            if(tool.UserId == renterId)
            {
                return Result<BookingResponse>.Failure("Can't rent your own tool!");
            }

            if(request.EndDate > tool.AvailableUntil)
            {
                return Result<BookingResponse>.Failure("Can't book this period, because tool isn't available!");
            }

            bool isOverLappingToolUnavailibility = await context.ToolUnavailabilities.AnyAsync(x => x.ToolId == tool.Id && x.StartDate <= request.EndDate && x.EndDate >= request.StartDate);
            if(isOverLappingToolUnavailibility)
            {
                return Result<BookingResponse>.Failure("Can't book this period, because this period is overlapping tool unavailability!");
            }

            bool isOverLappingBooking = await context.Bookings.AnyAsync(x => x.ToolId == tool.Id && x.StartDate <= request.EndDate && x.EndDate >= request.StartDate 
                                                    && x.Status != BookingStatus.CancelledByRenter && x.Status != BookingStatus.CancelledByOwner);
            if(isOverLappingBooking)
            {
                return Result<BookingResponse>.Failure("Can't book this period, because other user booked");
            }

            var booking = new Booking{ 
                ToolId = tool.Id,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                RenterId = renterId,
                Status = BookingStatus.Reserved,
                TotalPrice = ((request.EndDate.DayNumber - request.StartDate.DayNumber) + 1) * tool.DailyPrice,
                SecurityDeposit = tool.SecurityDeposit,

            };
            context.Bookings.Add(booking);
            await context.SaveChangesAsync();

            var response = new BookingResponse {
                Id = booking.Id,
                StartDate = booking.StartDate,
                EndDate = booking.EndDate,
                Status = booking.Status,
                TotalPrice = booking.TotalPrice,
                SecurityDeposit = booking.SecurityDeposit,
                CreatedAt = booking.CreatedAt,
                Tool = new ToolDto
                {
                    Id = tool.Id,
                    Name = tool.Name,
                    Description = tool.Description,
                    DailyPrice = tool.DailyPrice,
                    SecurityDeposit = tool.SecurityDeposit,
                    Location = tool.Location,
                    IsActive = tool.IsActive,
                    Category = tool.Category,
                    AvailableUntil = tool.AvailableUntil,
                },
                Renter = null 
            };

            return Result<BookingResponse>.Success(response);

        }
    }
}
