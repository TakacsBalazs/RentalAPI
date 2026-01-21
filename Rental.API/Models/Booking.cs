using Rental.API.Common;

namespace Rental.API.Models
{
    public class Booking : ISoftDelete
    {
        public int Id { get; set; }
        public int ToolId { get; set; }
        public Tool Tool { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public string RenterId { get; set; } = string.Empty;
        public User Renter { get; set; }
        public BookingStatus Status { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal SecurityDeposit { get; set; }
        public int PickupCode { get; set; }
        public int FailedPickupAttempts { get; set; } = 0;
        public bool IsLocked { get; set; } = false;
        public DateOnly? OriginalEndDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string? ClosingNote { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public enum BookingStatus
    {
        Reserved,
        Active,
        Completed,
        CancelledByRenter,
        CancelledByOwner
    }
}
