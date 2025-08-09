using System.ComponentModel.DataAnnotations;

namespace Faryma.Composer.Api.Features.ReviewOrderFeature.AddTrackUrl
{
    /// <summary>
    /// Запрос добавления в заказ ссылки на трек
    /// </summary>
    public sealed record AddTrackUrlRequest
    {
        /// <summary>
        /// Id заказа разбора трека
        /// </summary>
        public required long ReviewOrderId { get; set; }

        /// <summary>
        /// Ссылка на трек
        /// </summary>
        [Required]
        [Url(ErrorMessage = "Некорректная ссылка на трек")]
        public required string TrackUrl { get; set; }
    }
}