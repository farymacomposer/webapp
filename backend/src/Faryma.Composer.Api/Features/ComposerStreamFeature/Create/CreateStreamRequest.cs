using System.ComponentModel.DataAnnotations;
using Faryma.Composer.Infrastructure.Enums;

namespace Faryma.Composer.Api.Features.ComposerStreamFeature.Create
{
    /// <summary>
    /// Запрос создания стрима
    /// </summary>
    public sealed record CreateStreamRequest : IValidatableObject
    {
        /// <summary>
        /// Дата проведения стрима
        /// </summary>
        public required DateOnly EventDate { get; init; }

        /// <summary>
        /// Тип стрима
        /// </summary>
        [EnumDataType(typeof(ComposerStreamType), ErrorMessage = "Недопустимый тип стрима")]
        public required ComposerStreamType Type { get; init; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Type == ComposerStreamType.Unspecified)
            {
                yield return new ValidationResult("Недопустимый тип стрима");
            }

            if (EventDate < DateOnly.FromDateTime(DateTime.UtcNow))
            {
                yield return new ValidationResult("Нельзя создать стрим на прошедшую дату");
            }
        }
    }
}