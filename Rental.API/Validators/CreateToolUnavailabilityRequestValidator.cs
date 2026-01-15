using FluentValidation;
using Rental.API.Models.Requests;

namespace Rental.API.Validators
{
    public class CreateToolUnavailabilityRequestValidator : AbstractValidator<CreateToolUnavailabilityRequest>
    {
        public CreateToolUnavailabilityRequestValidator()
        {
            RuleFor(x => x.StartDate).NotEmpty().GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow));
            RuleFor(x => x.EndDate).NotEmpty().GreaterThanOrEqualTo(x => x.StartDate);
            RuleFor(x => x.ToolId).NotEmpty();
        }
    }
}
