namespace Faryma.Composer.Core.Features.ReviewOrderFeature.Commands
{
    /// <summary>
    /// Команда добавления/изменения ссылки на трек в заказе
    /// </summary>
    public sealed record AddTrackUrlCommand
    {
        /// <summary>
        /// Id заказа разбора трека
        /// </summary>
        public required long ReviewOrderId { get; init; }

        /// <summary>
        /// Ссылка на трек
        /// </summary>
        public required string TrackUrl { get; init; }
    }
}