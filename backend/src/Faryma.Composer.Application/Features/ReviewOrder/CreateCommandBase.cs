namespace Faryma.Composer.Application.Features.ReviewOrder
{
    /// <summary>
    /// Команда создания заказа
    /// </summary>
    public abstract record CreateCommandBase
    {
        /// <summary>
        /// Псевдоним пользователя
        /// </summary>
        public required string UserNickname { get; init; }

        /// <summary>
        /// Комментарий пользователя
        /// </summary>
        public required string? UserComment { get; init; }

        /// <summary>
        /// Ссылка на трек
        /// </summary>
        public required string? TrackUrl { get; init; }

        /// <summary>
        /// Длительность трека в секундах
        /// </summary>
        public required int? TrackDurationSeconds { get; init; }

        /// <summary>
        /// Id пользователя, создавшего заказ
        /// </summary>
        public required Guid CreatedByUserId { get; init; }
    }
}
