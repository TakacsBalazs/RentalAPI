using Rental.API.Common;

namespace Rental.API.Services
{
    public interface IPaymentService
    {
        Task<Result> LockBookingAmountAsync(string userId, decimal amount, int bookingId);
    }
}
