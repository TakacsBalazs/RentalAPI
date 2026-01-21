namespace Rental.API.Models.Requests
{
    public class ReportDamageCompleteBookingRequest
    {
        public decimal DamageAmount { get; set; }

        public string DamageDescription { get; set; } = string.Empty;
    }
}
