using Rental.API.Common;
using Rental.API.Data;
using Rental.API.Models.Responses;
using Rental.API.Models;

namespace Rental.API.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly AppDbContext context;
        public PaymentService(AppDbContext context)
        {
            this.context = context;
        }
        public async Task<Result> LockBookingAmountAsync(string userId, decimal amount, int bookingId)
        {
            var user = await context.Users.FindAsync(userId)!;
            if (user == null)
            {
                return Result.Failure("Invalid Renter User Id!");
            }

            if (user.Balance < amount)
            {
                return Result.Failure("Renter doesn't have enough money!");
            }

            user.Balance -= amount;
            user.LockedBalance += amount;

            var transaction = new Transaction
            {
                UserId = userId,
                Amount = amount,
                Type = TransactionType.BookingLock,
                BookingId = bookingId,
                Description = $"Lock booking payment (#{bookingId})",
                BalanceSnapshot = user.Balance,
                LockedBalanceSnapshot = user.LockedBalance
            };
            context.Transactions.Add(transaction);

            await context.SaveChangesAsync();

            return Result.Success();
        }
    }
}
