namespace Rental.API.Models.Responses
{
    public class TransactionResponse
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;

        public decimal Amount { get; set; }
        public TransactionType Type { get; set; }

        public DateTime CreatedAt { get; set; }
        public string Description { get; set; } = string.Empty;

        public int? BookingId { get; set; }

        public decimal BalanceSnapshot { get; set; }
        public decimal LockedBalanceSnapshot { get; set; }
    }
}
