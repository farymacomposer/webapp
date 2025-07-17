using System.ComponentModel.DataAnnotations;

namespace Faryma.Composer.Api.Features.ComposerStreamFeature.Find
{
    /// <summary>
    /// Запрос поиска стримов композитора
    /// </summary>
    public sealed class FindComposerStreamRequest : IValidatableObject
    {
        /// <summary>
        /// Начальная дата периода поиска
        /// </summary>
        public required DateOnly DateFrom { get; set; }

        /// <summary>
        /// Конечная дата периода поиска
        /// </summary>
        public required DateOnly DateTo { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (DateFrom > DateTo)
            {
                yield return new ValidationResult("Дата начала периода не может быть позже даты окончания");
            }
        }
    }
}