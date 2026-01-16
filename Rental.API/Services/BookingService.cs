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

        public async Task<Result<IEnumerable<BookingResponse>>> GetAllBookedToolsAsync(string renterId)
        {
            var response = await context.Bookings.Where(x => x.RenterId == renterId).Select(x => new BookingResponse
            {
                Id = x.Id,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                Status = x.Status,
                TotalPrice = x.TotalPrice,
                SecurityDeposit = x.SecurityDeposit,
                CreatedAt = x.CreatedAt,
                Tool = new ToolDto
                {
                    Id = x.Tool.Id,
                    Name = x.Tool.Name,
                    Description = x.Tool.Description,
                    DailyPrice = x.Tool.DailyPrice,
                    SecurityDeposit = x.Tool.SecurityDeposit,
                    Location = x.Tool.Location,
                    IsActive = x.Tool.IsActive,
                    Category = x.Tool.Category,
                    AvailableUntil = x.Tool.AvailableUntil,
                },
                Renter = null
            }).ToListAsync();
            return Result<IEnumerable<BookingResponse>>.Success(response);
        }

        public async Task<Result<IEnumerable<BookingResponse>>> GetAllOwnerBookingsAsync(string ownerId)
        {
            var response = await context.Bookings.Where(x => x.Tool.UserId == ownerId).Select(x => new BookingResponse
            {
                Id = x.Id,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                Status = x.Status,
                TotalPrice = x.TotalPrice,
                SecurityDeposit = x.SecurityDeposit,
                CreatedAt = x.CreatedAt,
                Tool = new ToolDto
                {
                    Id = x.Tool.Id,
                    Name = x.Tool.Name,
                    Description = x.Tool.Description,
                    DailyPrice = x.Tool.DailyPrice,
                    SecurityDeposit = x.Tool.SecurityDeposit,
                    Location = x.Tool.Location,
                    IsActive = x.Tool.IsActive,
                    Category = x.Tool.Category,
                    AvailableUntil = x.Tool.AvailableUntil,
                },
                Renter = new UserDto
                {
                    Id = x.Renter.Id,
                    FullName = x.Renter.FullName
                }
            }).ToListAsync();
            return Result<IEnumerable<BookingResponse>>.Success(response);
        }

        public async Task<Result<BookingDetailResponse>> GetBookingById(int id, string userId)
        {
            var book = await context.Bookings.Include(x => x.Renter).Include(x => x.Tool).ThenInclude(x => x.User).FirstOrDefaultAsync(x => x.Id == id);
            if (book == null)
            {
                return Result<BookingDetailResponse>.Failure("Invalid Id!");
            }

            bool isRenter = book.RenterId == userId;
            bool isOwner = book.Tool.UserId == userId;
            if (!isRenter && !isOwner)
            {
                return Result<BookingDetailResponse>.Failure("Can't see this booking!");
            }

            var response = new BookingDetailResponse
            {
                Id = book.Id,
                StartDate = book.StartDate,
                EndDate = book.EndDate,
                Status = book.Status,
                TotalPrice = book.TotalPrice,
                SecurityDeposit = book.SecurityDeposit,
                CreatedAt = book.CreatedAt,
                Tool = new ToolDto
                {
                    Id = book.Tool.Id,
                    Name = book.Tool.Name,
                    Description = book.Tool.Description,
                    DailyPrice = book.Tool.DailyPrice,
                    SecurityDeposit = book.Tool.SecurityDeposit,
                    Location = book.Tool.Location,
                    IsActive = book.Tool.IsActive,
                    Category = book.Tool.Category,
                    AvailableUntil = book.Tool.AvailableUntil,
                },
                Renter = new UserDto
                {
                    Id = book.Renter.Id,
                    FullName = book.Renter.FullName
                },
                Owner = new UserDto
                {
                    Id = book.Tool.User.Id,
                    FullName = book.Tool.User.FullName
                },
                AmIOwner = isOwner
            };
            return Result<BookingDetailResponse>.Success(response);
        }
    }
}
