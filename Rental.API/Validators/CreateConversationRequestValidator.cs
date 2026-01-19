using FluentValidation;
using Rental.API.Models.Requests;

namespace Rental.API.Validators
{
    public class CreateConversationRequestValidator : AbstractValidator<CreateConversationRequest>
    {
        public CreateConversationRequestValidator() { 
            RuleFor(x => x.TargetUserId).NotEmpty();
        }
    }
}
