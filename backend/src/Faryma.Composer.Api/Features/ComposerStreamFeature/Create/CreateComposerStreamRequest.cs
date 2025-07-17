using System.ComponentModel.DataAnnotations;
using Faryma.Composer.Infrastructure.Enums;

namespace Faryma.Composer.Api.Features.ComposerStreamFeature.Create
{
    /// <summary>
    /// Запрос создания стрима
    /// </summary>
    public sealed class CreateComposerStreamRequest : IValidatableObject
    {
        /// <summary>
        /// Дата проведения стрима
        /// </summary>
        public required DateOnly EventDate { get; set; }

        /// <summary>
        /// Тип стрима
        /// </summary>
        [EnumDataType(typeof(ComposerStreamType), ErrorMessage = "Недопустимый тип стрима")]
        public required ComposerStreamType Type { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (EventDate < DateOnly.FromDateTime(DateTime.UtcNow))
            {
                yield return new ValidationResult("Дата стрима не может быть в прошлом, выберите другую дату");
            }
        }
    }
}