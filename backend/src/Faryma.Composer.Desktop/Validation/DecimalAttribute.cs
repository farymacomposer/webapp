using System.ComponentModel.DataAnnotations;
using Faryma.Composer.Desktop.Utils;

namespace Faryma.Composer.Desktop.Validation
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class DecimalAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object? value, ValidationContext context)
        {
            if (!StringParser.TryParseNumber(value, out float _))
            {
                return new ValidationResult($"Значение {context.DisplayName}, должно быть числом");
            }

            return ValidationResult.Success!;
        }
    }
}