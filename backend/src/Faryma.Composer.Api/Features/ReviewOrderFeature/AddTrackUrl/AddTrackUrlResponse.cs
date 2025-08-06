namespace Faryma.Composer.Api.Features.ReviewOrderFeature.AddTrackUrl
{
    /// <summary>
    /// Ответ на запрос добавления ссылки на трек
    /// </summary>
    public sealed record AddTrackUrlResponse
    {
        /// <summary>
        /// Id заказа на разбор трека
        /// </summary>
        public required long ReviewOrderId { get; init; }

        /// <summary>
        /// Ссылка на трек
        /// </summary>
        public required string TrackUrl { get; init; }
    }
}