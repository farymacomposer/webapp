using System.ComponentModel.DataAnnotations;

namespace Faryma.Composer.Contracts.Api.Features.ReviewOrder.Create
{
    /// <summary>
    /// Общие поля запроса создания заказа на разбор
    /// </summary>
    public abstract record CreateReviewOrderRequestBase : IValidatableObject
    {
        /// <summary>
        /// Псевдоним пользователя
        /// </summary>
        [Required]
        [StringLength(40, MinimumLength = 1, ErrorMessage = "Длина псевдонима должна быть в пределах от 1 до 40 символов")]
        public required string Nickname { get; init; }

        /// <summary>
        /// Ссылка на трек
        /// </summary>
        [Url(ErrorMessage = "Некорректная ссылка на трек")]
        public string? TrackUrl { get; init; }

        /// <summary>
        /// Длительность трека в секундах
        /// </summary>
        public int? TrackDurationSeconds { get; init; }

        /// <summary>
        /// Комментарий пользователя
        /// </summary>
        [StringLength(200, ErrorMessage = "Максимальная длина комментария 200 символов")]
        public string? UserComment { get; init; }

        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (TrackDurationSeconds.HasValue && !(TrackDurationSeconds > 0 && TrackDurationSeconds <= Globals.MaxTrackDurationSeconds))
            {
                yield return new ValidationResult("Длительность трека должна быть в пределах от 1 секунды до 15 минут");
            }

            bool hasUrl = !string.IsNullOrWhiteSpace(TrackUrl);
            bool hasDuration = TrackDurationSeconds > 0;

            if (hasUrl != hasDuration)
            {
                yield return new ValidationResult("Если указаны ссылка на трек или длительность, оба поля должны быть заполнены");
            }
        }
    }
}
