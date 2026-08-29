using System.ComponentModel.DataAnnotations;

namespace Faryma.Composer.Api.Features.ReviewOrder.AddTrackUrl
{
    /// <summary>
    /// Запрос добавления ссылки на трек в заказ
    /// </summary>
    public sealed record AddTrackUrlRequest
    {
        /// <summary>
        /// Id заказа разбора трека
        /// </summary>
        [Range(1, long.MaxValue, ErrorMessage = "Id заказа должен быть больше нуля")]
        public required long ReviewOrderId { get; init; }

        /// <summary>
        /// Ссылка на трек
        /// </summary>
        [Required]
        [Url(ErrorMessage = "Некорректная ссылка на трек")]
        public required string TrackUrl { get; init; }

        /// <summary>
        /// Длительность трека в секундах
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Длительность трека должна быть больше нуля")]
        public required int TrackDurationSeconds { get; init; }
    }
}
