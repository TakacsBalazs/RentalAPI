using FluentValidation;
using Rental.API.Models.Requests;

namespace Rental.API.Validators
{
    public class CreateMessageRequestValidator :  AbstractValidator<CreateMessageRequest>
    {
        public CreateMessageRequestValidator() { 
            RuleFor(x => x.Text).NotEmpty();
        }

    }
}
