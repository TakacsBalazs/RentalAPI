using FluentValidation;
using Rental.API.Models.Requests;

namespace Rental.API.Validators
{
    public class LogoutRequestValidator : AbstractValidator<LogoutRequest>
    {
        public LogoutRequestValidator() { 
            RuleFor(x => x.RefreshToken).NotEmpty();
        }
    }
}
