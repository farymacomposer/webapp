using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;

namespace Faryma.Composer.Api.Contracts.Features.Auth.Options
{
    public sealed record AdminBootstrapOptions : IValidatableObject
    {
        [ConfigurationKeyName("COMPOSER")]
        [Required]
        public required AdminBootstrapAccountOptions Composer { get; init; }

        [ConfigurationKeyName("MODERATOR")]
        [Required]
        public required AdminBootstrapAccountOptions Moderator { get; init; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            foreach (ValidationResult result in ValidateAccount(Composer))
            {
                yield return result;
            }

            foreach (ValidationResult result in ValidateAccount(Moderator))
            {
                yield return result;
            }

            if (string.Equals(Composer?.UserName, Moderator?.UserName, StringComparison.OrdinalIgnoreCase))
            {
                yield return new ValidationResult("Логины для COMPOSER и MODERATOR должны отличаться");
            }
        }

        private static IEnumerable<ValidationResult> ValidateAccount(AdminBootstrapAccountOptions account)
        {
            ValidationContext context = new(account);
            List<ValidationResult> validationResults = [];
            if (Validator.TryValidateObject(account, context, validationResults, true))
            {
                return Enumerable.Empty<ValidationResult>();
            }

            return validationResults;
        }
    }
}
