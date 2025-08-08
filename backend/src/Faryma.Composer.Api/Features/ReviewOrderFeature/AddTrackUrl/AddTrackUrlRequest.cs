using System.ComponentModel.DataAnnotations;
using Faryma.Composer.Core.Features.ReviewOrderFeature.Commands;

namespace Faryma.Composer.Api.Features.ReviewOrderFeature.AddTrackUrl
{
    /// <summary>
    /// Запрос добавления ссылки на трек
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

        public AddTrackUrlCommand Map()
        {
            return new()
            {
                ReviewOrderId = ReviewOrderId,
                TrackUrl = TrackUrl,
            };
        }
    }
}