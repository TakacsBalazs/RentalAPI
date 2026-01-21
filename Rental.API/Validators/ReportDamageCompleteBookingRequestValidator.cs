using FluentValidation;
using Rental.API.Models.Requests;

namespace Rental.API.Validators
{
    public class ReportDamageCompleteBookingRequestValidator : AbstractValidator<ReportDamageCompleteBookingRequest>
    {
        public ReportDamageCompleteBookingRequestValidator()
        {
            RuleFor(x => x.DamageAmount).GreaterThan(0);
            RuleFor(x => x.DamageDescription).NotEmpty().MaximumLength(1000);
        }
    }
}
