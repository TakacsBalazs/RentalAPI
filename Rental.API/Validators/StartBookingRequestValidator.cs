using FluentValidation;
using Rental.API.Models.Requests;

namespace Rental.API.Validators
{
    public class StartBookingRequestValidator : AbstractValidator<StartBookingRequest>
    {
        public StartBookingRequestValidator() {
            RuleFor(x => x.PickupCode).NotEmpty();
        }
    }
}
