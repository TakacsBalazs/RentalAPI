using FluentValidation;
using Rental.API.Models.Requests;

namespace Rental.API.Validators
{
    public class GetRatingsRequestValidator : AbstractValidator<GetRatingsRequest>
    {
        public GetRatingsRequestValidator() { 
            RuleFor(x => x.RatedUserId).NotEmpty();
        }
    }
}
