namespace Rental.API.Models.Responses
{
    public class ReportDamageCompleteBookingResponse
    {
        public decimal PaidDamage { get; set; }

        public decimal RequestedDamage { get; set; }

        public bool IsFullyCovered { get; set; }
    }
}
