using FluentValidation;
using Rental.API.Models.Requests;

namespace Rental.API.Validators
{
    public class GetToolUnavailabilitiesRequestValidator : AbstractValidator<GetToolUnavailabilitiesRequest>
    {
        public GetToolUnavailabilitiesRequestValidator()
        {
            RuleFor(x => x.ToolId).GreaterThan(0);
            RuleFor(x => x.To).GreaterThanOrEqualTo(x => x.From).When(x => x.From != null && x.To != null);
        }
    }
}
