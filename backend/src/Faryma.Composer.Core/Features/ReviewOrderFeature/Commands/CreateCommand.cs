using Faryma.Composer.Infrastructure.Enums;

namespace Faryma.Composer.Core.Features.ReviewOrderFeature.Commands
{
    /// <summary>
    /// Команда создания заказа
    /// </summary>
    public sealed record CreateCommand
    {
        /// <summary>
        /// Псевдоним пользователя
        /// </summary>
        public required string Nickname { get; init; }

        /// <summary>
        /// Тип заказа
        /// </summary>
        public required ReviewOrderType OrderType { get; init; }

        /// <summary>
        /// Ссылка на трек
        /// </summary>
        public string? TrackUrl { get; init; }

        /// <summary>
        /// Сумма платежа
        /// </summary>
        public decimal? PaymentAmount { get; init; }

        /// <summary>
        /// Комментарий пользователя
        /// </summary>
        public string? UserComment { get; init; }
    }
}