using FluentValidation;
using Rental.API.Models.Requests;

namespace Rental.API.Validators
{
    public class GetToolBookingRequestValidator : AbstractValidator<GetToolBookingRequest>
    {
        public GetToolBookingRequestValidator() {
            RuleFor(x => x.ToolId).GreaterThan(0);
        }
    }
}
