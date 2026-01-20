using FluentValidation;
using Rental.API.Models.Requests;

namespace Rental.API.Validators
{
    public class CreateRatingRequestValidator : AbstractValidator<CreateRatingRequest>
    {
        public CreateRatingRequestValidator() { 
            RuleFor(x => x.RatedUserId).NotEmpty();
            RuleFor(x => x.Rate).GreaterThan(0).LessThan(6);
        }

    }
}
