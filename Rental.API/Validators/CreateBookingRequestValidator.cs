using FluentValidation;
using Rental.API.Models.Requests;

namespace Rental.API.Validators
{
    public class CreateBookingRequestValidator : AbstractValidator<CreateBookingRequest>
    {
        public CreateBookingRequestValidator() {
            RuleFor(x => x.ToolId).GreaterThan(0);
            RuleFor(x => x.StartDate).NotEmpty().GreaterThan(DateOnly.FromDateTime(DateTime.UtcNow));
            RuleFor(x => x.EndDate).NotEmpty().GreaterThanOrEqualTo(x => x.StartDate);
        }
    }
}
