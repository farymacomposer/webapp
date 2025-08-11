using Faryma.Composer.Api.Features.CommonDto;

namespace Faryma.Composer.Api.Features.ReviewOrderFeature.AddTrackUrl
{
    /// <summary>
    /// Ответ на запрос добавления в заказ ссылки на трек
    /// </summary>
    public sealed record AddTrackUrlResponse
    {
        /// <summary>
        /// Заказ разбора трека
        /// </summary>
        public required ReviewOrderDto ReviewOrder { get; init; }
    }
}