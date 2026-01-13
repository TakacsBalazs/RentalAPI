using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Rental.API.Common;

namespace Rental.API.Extensions
{
    public static class ValidationExtensions
    {
        public static async Task<Result> ValidateRequestAsync<T>(this IServiceProvider provider, T request)
        {
            var validator = provider.GetService<IValidator<T>>();

            if (validator == null) return Result.Success();

            var validationResult = await validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return Result.Failure(errors);
            }

            return Result.Success();
        }
    }
}
