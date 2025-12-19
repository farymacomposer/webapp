namespace Faryma.Composer.Contracts.Application.Features.ReviewOrder.Commands
{
    /// <summary>
    /// Команда создания заказа
    /// </summary>
    public abstract record CreateCommandBase
    {
        /// <summary>
        /// Псевдоним пользователя
        /// </summary>
        public required string Nickname { get; init; }

        /// <summary>
        /// Ссылка на трек
        /// </summary>
        public required string? TrackUrl { get; init; }

        /// <summary>
        /// Комментарий пользователя
        /// </summary>
        public required string? UserComment { get; init; }
    }
}