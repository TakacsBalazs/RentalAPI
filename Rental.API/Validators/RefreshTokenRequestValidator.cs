using FluentValidation;
using Rental.API.Models.Requests;

namespace Rental.API.Validators
{
    public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
    {
        public RefreshTokenRequestValidator() {
            RuleFor(x => x.RefreshToken).NotEmpty();
        }
    }
}
