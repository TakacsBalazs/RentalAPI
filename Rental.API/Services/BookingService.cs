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
        private readonly IPaymentService paymentService;
        private readonly INotificationService notificationService;

        public BookingService(AppDbContext context, IServiceProvider serviceProvider, IPaymentService paymentService, INotificationService notificationService)
        {
            this.context = context;
            this.serviceProvider = serviceProvider;
            this.paymentService = paymentService;
            this.notificationService = notificationService;
        }

        public async Task<Result<BookingResponse>> CreateBookingAsync(CreateBookingRequest request, string renterId)
        {
            var validate = await serviceProvider.ValidateRequestAsync<CreateBookingRequest>(request);
            if (!validate.IsSuccess)
            {
                return Result<BookingResponse>.Failure(validate.Errors);
            }

            var tool = await context.Tools.Include(x => x.User).FirstOrDefaultAsync(x => x.Id == request.ToolId);
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

            var booking = new Booking {
                ToolId = tool.Id,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                RenterId = renterId,
                Status = BookingStatus.Reserved,
                TotalPrice = ((request.EndDate.DayNumber - request.StartDate.DayNumber) + 1) * tool.DailyPrice,
                SecurityDeposit = tool.SecurityDeposit,
                PickupCode = Random.Shared.Next(1000, 9999)

            };

            using var dbTransaction = await context.Database.BeginTransactionAsync();
            try
            {
                context.Bookings.Add(booking);
                await context.SaveChangesAsync();
                decimal amount = booking.TotalPrice + booking.SecurityDeposit;

                var paymentResult = await paymentService.LockBookingAmountAsync(renterId, amount, booking.Id);
                if (!paymentResult.IsSuccess)
                {
                    return Result<BookingResponse>.Failure(paymentResult.Errors);
                }

                await notificationService.SendNotificationAsync(tool.UserId, "New Booking!", $"Your {tool.Name} has been booked #{booking.Id} from {booking.StartDate:yyyy-MM-dd} to {booking.EndDate:yyyy-MM-dd}.");

                await dbTransaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await dbTransaction.RollbackAsync();
                return Result<BookingResponse>.Failure("System error: " + ex.Message);
            }

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
                AmIOwner = isOwner,
                PickupCode = isRenter ? book.PickupCode : null
            };
            return Result<BookingDetailResponse>.Success(response);
        }

        public async Task<Result<IEnumerable<BookingResponse>>> GetAllBookingsByToolAsync(GetToolBookingRequest request, string userId)
        {
            var validate = await serviceProvider.ValidateRequestAsync<GetToolBookingRequest>(request);
            if (!validate.IsSuccess)
            {
                return Result<IEnumerable<BookingResponse>>.Failure(validate.Errors);
            }

            var tool = await context.Tools.FindAsync(request.ToolId);
            if(tool == null)
            {
                return Result<IEnumerable<BookingResponse>>.Failure("Invalid Tool Id!");
            }

            if(tool.UserId !=  userId)
            {
                return Result<IEnumerable<BookingResponse>>.Failure("Can't see these bookings!");
            }

            var response = await context.Bookings.Where(x => x.ToolId == request.ToolId).Select(x => new BookingResponse
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
                Renter = new UserDto
                {
                    Id = x.Renter.Id,
                    FullName = x.Renter.FullName
                }
            }).ToListAsync();
            return Result<IEnumerable<BookingResponse>>.Success(response);
        }

        public async Task<Result> StartTheBookingAsync(int id, string userId, StartBookingRequest request)
        {
            var validate = await serviceProvider.ValidateRequestAsync<StartBookingRequest>(request);
            if (!validate.IsSuccess)
            {
                return Result.Failure(validate.Errors);
            }

            var booking = await context.Bookings.Include(x => x.Tool).FirstOrDefaultAsync(x => x.Id == id);
            if(booking == null)
            {
                return Result.Failure("Invalid Id!");
            }

            if (booking.IsLocked)
            {
                return Result.Failure("Locked booking, contact us!");
            }

            if(booking.Tool.UserId != userId)
            {
                return Result.Failure("Can't do this!");
            }

            if(booking.StartDate > DateOnly.FromDateTime(DateTime.UtcNow))
            {
                return Result.Failure("Can't start the booking yet!");
            }

            if(booking.Status != BookingStatus.Reserved)
            {
                return Result.Failure("Can't start the booking!");
            }

            if(booking.PickupCode != request.PickupCode)
            {
                booking.FailedPickupAttempts++;
                if(booking.FailedPickupAttempts >= 5)
                {
                    booking.IsLocked = true;
                    await context.SaveChangesAsync();
                    return Result.Failure("Locked!");
                }
                await context.SaveChangesAsync();
                return Result.Failure($"{5-booking.FailedPickupAttempts} tries left!");
            }

            await notificationService.SendNotificationAsync(booking.RenterId, "Booking Started!", $"Pickup successful! You have the {booking.Tool.Name} #{booking.Id}. Please return it on time! ({booking.EndDate:yyyy-MM-dd})");

            booking.Status = BookingStatus.Active;
            await context.SaveChangesAsync();
            return Result.Success();
        }

        public async Task<Result> DeleteBookingByIdAsync(int id, string userId)
        {
            var booking = await context.Bookings.Include(x => x.Tool).FirstOrDefaultAsync(x => x.Id == id);
            if(booking == null)
            {
                return Result.Failure("Invalid Booking Id!");
            }

            bool isRenter = booking.RenterId == userId;
            bool isOwner = booking.Tool.UserId == userId;
            if(!isOwner && !isRenter)
            {
                return Result.Failure("Can't remove this booking!");
            }

            if(booking.Status != BookingStatus.Reserved)
            {
                return Result.Failure("Can't cancel this booking!");
            }

            if(DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1) >= booking.StartDate)
            {
                return Result.Failure("Can't cancel this booking anymore!");
            }

            using var dbTransaction = await context.Database.BeginTransactionAsync();
            try
            {
                var paymentResult = await paymentService.CancellationUnlockBookingAmountAsync(booking.RenterId, (booking.TotalPrice + booking.SecurityDeposit), booking.Id);
                if (!paymentResult.IsSuccess)
                {
                    return Result.Failure(paymentResult.Errors);
                }

                var notification = new Notification
                {
                    Title = "Booking Cancelled!",
                };

                string notificationUserId = "";
                string notificationMessage = "";
                if (isOwner)
                {
                    notificationUserId = booking.RenterId;
                    notificationMessage = $"The owner cancelled booking #{booking.Id} for {booking.Tool.Name} from {booking.StartDate:yyyy-MM-dd} to {booking.EndDate:yyyy-MM-dd}. Your funds have been released.";
                    booking.Status = BookingStatus.CancelledByOwner;
                }
                else if (isRenter)
                {
                    notificationUserId = booking.Tool.UserId;
                    notificationMessage = $"The renter cancelled booking #{booking.Id} for {booking.Tool.Name} from {booking.StartDate:yyyy-MM-dd} to {booking.EndDate:yyyy-MM-dd}. The tool is available for others.";
                    booking.Status = BookingStatus.CancelledByRenter;
                }
                await notificationService.SendNotificationAsync(notificationUserId, "Booking Cancelled!", notificationMessage);

                booking.IsDeleted = true;
                booking.DeletedAt = DateTime.UtcNow;

                await context.SaveChangesAsync();
                await dbTransaction.CommitAsync();
            }
            catch (Exception ex)
            {

                await dbTransaction.RollbackAsync();
                return Result.Failure("System error: " + ex.Message);
            }

            return Result.Success();
        }

        public async Task<Result> CompleteTheBookingAsync(int id, string userId)
        {
            var booking = await context.Bookings.Include(x => x.Tool).FirstOrDefaultAsync(x => x.Id == id);
            if(booking == null)
            {
                return Result.Failure("Invalid Booking Id!");
            }

            if(booking.Tool.UserId != userId)
            {
                return Result.Failure("Can't complete this booking!");
            }

            if(booking.Status != BookingStatus.Active)
            {
                return Result.Failure("Booking isn't active!");
            }


            using var dbTransaction = await context.Database.BeginTransactionAsync();
            try
            {
                var paymentResult = await paymentService.CompleteBookingAmountAsync(booking.Tool.UserId, booking.RenterId, booking.TotalPrice, booking.SecurityDeposit, booking.Id);
                if(!paymentResult.IsSuccess)
                {
                    return Result.Failure(paymentResult.Errors);
                }

                var today = DateOnly.FromDateTime(DateTime.UtcNow);
                if (today < booking.EndDate)
                {
                    booking.OriginalEndDate = booking.EndDate;
                    booking.EndDate = today;
                }
                booking.Status = BookingStatus.Completed;
                booking.ReturnDate = DateTime.UtcNow;
                await context.SaveChangesAsync();

                await notificationService.SendNotificationAsync(booking.RenterId, "Booking Completed!", $"Booking #{booking.Id} for {booking.Tool.Name} was completed on {DateTime.UtcNow:yyyy-MM-dd}. Your security deposit has been released. Thanks for renting!");
                await dbTransaction.CommitAsync();
            }
            catch (Exception ex)
            {

                await dbTransaction.RollbackAsync();
                return Result.Failure("System error: " + ex.Message);
            }
            return Result.Success();
        }

        public async Task<Result<ReportDamageCompleteBookingResponse>> ReportDamageCompleteBookingAsync(int id, string userId, ReportDamageCompleteBookingRequest request)
        {
            var validate = await serviceProvider.ValidateRequestAsync<ReportDamageCompleteBookingRequest>(request);
            if (!validate.IsSuccess)
            {
                return Result<ReportDamageCompleteBookingResponse>.Failure(validate.Errors);
            }

            var booking = await context.Bookings.Include(x => x.Tool).FirstOrDefaultAsync(x => x.Id == id);
            if (booking == null)
            {
                return Result<ReportDamageCompleteBookingResponse>.Failure("Invalid Booking Id!");
            }

            if (booking.Tool.UserId != userId)
            {
                return Result<ReportDamageCompleteBookingResponse>.Failure("Can't complete this booking!");
            }

            if (booking.Status != BookingStatus.Active)
            {
                return Result<ReportDamageCompleteBookingResponse>.Failure("Booking isn't active!");
            }

            using var dbTransaction = await context.Database.BeginTransactionAsync();
            try
            {
                var paymentResult = await paymentService.ReportDamageCompleteBookingAmountAsync(booking.Tool.UserId, booking.RenterId, booking.TotalPrice,
                    booking.SecurityDeposit, request.DamageAmount, request.DamageDescription, booking.Id);
                if (!paymentResult.IsSuccess)
                {
                    return Result<ReportDamageCompleteBookingResponse>.Failure(paymentResult.Errors);
                }

                var today = DateOnly.FromDateTime(DateTime.UtcNow);
                if (today < booking.EndDate)
                {
                    booking.OriginalEndDate = booking.EndDate;
                    booking.EndDate = today;
                }

                booking.ClosingNote = request.DamageDescription;
                booking.Status = BookingStatus.Completed;
                booking.ReturnDate = DateTime.UtcNow;
                await context.SaveChangesAsync();

                await notificationService.SendNotificationAsync(booking.RenterId, "Damage Reported!", $"Damage reported for booking #{booking.Id} ({booking.Tool.Name}). You were charged {request.DamageAmount:C}. Reason: {request.DamageDescription}");
                await dbTransaction.CommitAsync();

                return Result<ReportDamageCompleteBookingResponse>.Success(paymentResult.Data);
            }
            catch (Exception ex)
            {
                await dbTransaction.RollbackAsync();
                return Result<ReportDamageCompleteBookingResponse>.Failure("System error: " + ex.Message);
            }

        }

        public async Task<Result> CancelExpiredBookingAsync(int id)
        {
            var booking = await context.Bookings.Include(x => x.Tool).FirstOrDefaultAsync(x => x.Id == id);
            if(booking == null)
            {
                return Result.Failure("Invalid Booking Id!");
            }

            if(booking.Status != BookingStatus.Reserved)
            {
                return Result.Failure("Can't be expired!");
            }

            using var dbTransaction = await context.Database.BeginTransactionAsync();
            try
            {
                var paymentResult = await paymentService.CancellationUnlockBookingAmountAsync(booking.RenterId, (booking.TotalPrice + booking.SecurityDeposit), booking.Id);
                if (!paymentResult.IsSuccess)
                {
                    return Result.Failure(paymentResult.Errors);
                }

                booking.Status = BookingStatus.Expired;
                booking.IsDeleted = true;
                booking.DeletedAt = DateTime.UtcNow;

                await notificationService.SendNotificationAsync(booking.RenterId, "Booking Expired!", $"The system automatically cancelled booking #{booking.Id} for '{booking.Tool.Name}' because it was not started on time. Your funds have been released.");

                await notificationService.SendNotificationAsync(booking.Tool.UserId, "Booking Expired!", $"The system automatically cancelled booking #{booking.Id} for '{booking.Tool.Name}' because it was not started on time.");

                await context.SaveChangesAsync();
                await dbTransaction.CommitAsync();
                return Result.Success();
            }
            catch (Exception ex)
            {
                await dbTransaction.RollbackAsync();
                return Result.Failure("System error: " + ex.Message);
            }
        }
    }
}
