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
    }
}
