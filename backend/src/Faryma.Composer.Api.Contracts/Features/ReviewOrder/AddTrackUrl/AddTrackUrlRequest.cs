using System.ComponentModel.DataAnnotations;
using Faryma.Composer.Domain;

namespace Faryma.Composer.Api.Contracts.Features.ReviewOrder.AddTrackUrl
{
    /// <summary>
    /// Запрос добавления в заказ ссылки на трек
    /// </summary>
    public sealed record AddTrackUrlRequest
    {
        /// <summary>
        /// Id заказа разбора трека
        /// </summary>
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
        [Range(1, Globals.MaxTrackDurationSeconds, ErrorMessage = "Длительность трека должна быть в пределах от 1 секунды до 15 минут")]
        public required int TrackDurationSeconds { get; init; }
    }
}
