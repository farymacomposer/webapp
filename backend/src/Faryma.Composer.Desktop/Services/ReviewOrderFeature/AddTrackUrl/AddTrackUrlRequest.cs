namespace Faryma.Composer.Desktop.Services.ReviewOrderFeature.AddTrackUrl
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
        public required string? TrackUrl { get; init; }
    }
}