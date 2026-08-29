using System.ComponentModel.DataAnnotations;
using Faryma.Composer.Api.SharedDto;

namespace Faryma.Composer.Api.Features.ReviewOrder.AddTrackUrl
{
    /// <summary>
    /// Ответ на запрос добавления в заказ ссылки на трек
    /// </summary>
    public sealed record AddTrackUrlResponse
    {
        /// <summary>
        /// Заказ разбора трека
        /// </summary>
        [Required]
        public required ReviewOrderDto ReviewOrder { get; init; }
    }
}
