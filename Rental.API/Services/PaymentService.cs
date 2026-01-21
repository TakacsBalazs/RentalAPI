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

        public async Task<Result> CancellationUnlockBookingAmountAsync(string userId, decimal amount, int bookingId)
        {
            var user = await context.Users.FindAsync(userId);
            if (user == null)
            {
                return Result.Failure("Invalid Renter User Id!");
            }
            
            user.Balance += amount;
            user.LockedBalance -= amount;

            var transaction = new Transaction
            {
                UserId = userId,
                Amount = amount,
                Type = TransactionType.BookingCancellationUnlock,
                BookingId = bookingId,
                Description = $"Cancelation unlock booking payment (#{bookingId})",
                BalanceSnapshot = user.Balance,
                LockedBalanceSnapshot = user.LockedBalance
            };
            context.Transactions.Add(transaction);

            await context.SaveChangesAsync();

            return Result.Success();
        }

        public async Task<Result> CompleteBookingAmountAsync(string toolUserId, string renterId, decimal totalPrice, decimal securityDeposit, int bookingId)
        {
            var toolUser = await context.Users.FindAsync(toolUserId);
            if(toolUser == null)
            {
                return Result.Failure("Invalid Tool User Id!");
            }

            var renter = await context.Users.FindAsync(renterId);
            if(renter == null)
            {
                return Result.Failure("Invalid Renter Id!");
            }

            toolUser.Balance += totalPrice;
            var toolUserTransaction = new Transaction
            {
                UserId = toolUserId,
                Amount = totalPrice,
                Type = TransactionType.RentalIncome,
                BookingId = bookingId,
                Description = $"Rental income booking payment (#{bookingId})",
                BalanceSnapshot = toolUser.Balance,
                LockedBalanceSnapshot = toolUser.LockedBalance
            };
            context.Transactions.Add(toolUserTransaction);

            renter.LockedBalance -= totalPrice;
            var renterFeePaymentTransaction = new Transaction
            {
                UserId = renterId,
                Amount = totalPrice,
                Type = TransactionType.RentalFeePayment,
                BookingId = bookingId,
                Description = $"Rental fee booking payment (#{bookingId})",
                BalanceSnapshot = renter.Balance,
                LockedBalanceSnapshot = renter.LockedBalance
            };
            context.Transactions.Add(renterFeePaymentTransaction);

            renter.LockedBalance -= securityDeposit;
            renter.Balance += securityDeposit;
            var renterSecurityDespositRefundTransaction = new Transaction
            {
                UserId = renterId,
                Amount = securityDeposit,
                Type = TransactionType.SecurityDepositRefund,
                BookingId = bookingId,
                Description = $"Rental security desposit refund booking payment (#{bookingId})",
                BalanceSnapshot = renter.Balance,
                LockedBalanceSnapshot = renter.LockedBalance
            };
            context.Transactions.Add(renterSecurityDespositRefundTransaction);

            await context.SaveChangesAsync();

            return Result.Success();
        }
    }
}
