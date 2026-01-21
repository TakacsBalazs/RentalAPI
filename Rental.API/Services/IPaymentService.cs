using Rental.API.Common;
using Rental.API.Models.Responses;

namespace Rental.API.Services
{
    public interface IPaymentService
    {
        Task<Result> LockBookingAmountAsync(string userId, decimal amount, int bookingId);

        Task<Result> CancellationUnlockBookingAmountAsync(string userId, decimal amount, int bookingId);

        Task<Result> CompleteBookingAmountAsync(string toolUserId, string renterId, decimal totalPrice, decimal securityDeposit, int bookingId);

        Task<Result<ReportDamageCompleteBookingResponse>> ReportDamageCompleteBookingAmountAsync(string toolUserId, string renterId, decimal totalPrice, decimal securityDeposit, decimal damageAmount, string damageDescription, int bookingId);
    }
}
