using FluentValidation;
using Rental.API.Models.Requests;

namespace Rental.API.Validators
{
    public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
    {
        public UpdateUserRequestValidator() {
            RuleFor(x => x.FullName).NotEmpty().MaximumLength(100);
        }
    }
}
