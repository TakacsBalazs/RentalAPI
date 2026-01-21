using Rental.API.Common;

namespace Rental.API.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public User User { get; set; }

        public decimal Amount { get; set; }
        public TransactionType Type { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string Description { get; set; } = string.Empty;

        public int? BookingId { get; set; }
        public Booking? Booking { get; set; }

        public decimal BalanceSnapshot { get; set; }
        public decimal LockedBalanceSnapshot { get; set; }
    }

    public enum TransactionType
    {
        BalanceTopUp = 1,
        Withdraw = 2,

        BookingLock = 10,
        BookingCancellationUnlock = 11,

        RentalFeePayment = 20,
        RentalIncome = 21,
        SecurityDepositRefund = 22,

        DamagePenalty = 30,
        PenaltyIncome = 31,
    }
}
