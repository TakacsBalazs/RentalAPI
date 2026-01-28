using Rental.API.Common;
using Rental.API.Models;
using Rental.API.Models.Requests;
using Rental.API.Models.Responses;

namespace Rental.API.Services
{
    public interface IBookingService
    {
        Task<Result<BookingResponse>> CreateBookingAsync(CreateBookingRequest request, string renterId);

        Task<Result<IEnumerable<BookingResponse>>> GetAllBookedToolsAsync(string renterId);

        Task<Result<IEnumerable<BookingResponse>>> GetAllOwnerBookingsAsync(string ownerId);

        Task<Result<BookingDetailResponse>> GetBookingById(int id, string userId);

        Task<Result<IEnumerable<BookingResponse>>> GetAllBookingsByToolAsync(GetToolBookingRequest request, string userId);

        Task<Result> StartTheBookingAsync(int id, string userId, StartBookingRequest request);

        Task<Result> DeleteBookingByIdAsync(int id, string userId);

        Task<Result> CompleteTheBookingAsync(int id, string userId);

        Task<Result<ReportDamageCompleteBookingResponse>> ReportDamageCompleteBookingAsync(int id, string userId, ReportDamageCompleteBookingRequest request);

        Task<Result> CancelExpiredBookingAsync(int id);
    }
}
