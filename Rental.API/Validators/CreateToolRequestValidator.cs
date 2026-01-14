using FluentValidation;
using Rental.API.Models.Requests;

namespace Rental.API.Validators
{
    public class CreateToolRequestValidator : AbstractValidator<CreateToolRequest>
    {

        public CreateToolRequestValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MinimumLength(10).MaximumLength(100);
            RuleFor(x => x.Description).NotEmpty().MinimumLength(20).MaximumLength(1000);
            RuleFor(x => x.DailyPrice).NotEmpty().GreaterThan(0).PrecisionScale(18, 2, true);
            RuleFor(x => x.SecurityDeposit).NotEmpty().GreaterThan(0).PrecisionScale(18, 2, true);
            RuleFor(x => x.Location).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Category).NotEmpty().IsInEnum();

        }

    }
}
