using Faryma.Composer.Infrastructure.Enums;

namespace Faryma.Composer.Desktop.Services.ReviewOrderFeature.Dto
{
    /// <summary>
    /// Запрос создания заказа на разбор
    /// </summary>
    public sealed record CreateReviewOrderRequest
    {
        /// <summary>
        /// Псевдоним пользователя
        /// </summary>
        public required string? Nickname { get; init; }

        /// <summary>
        /// Тип заказа
        /// </summary>
        public required ReviewOrderType OrderType { get; init; }

        /// <summary>
        /// Ссылка на трек
        /// </summary>
        public required string? TrackUrl { get; init; }

        /// <summary>
        /// Сумма платежа
        /// </summary>
        public required decimal? PaymentAmount { get; init; }

        /// <summary>
        /// Комментарий пользователя
        /// </summary>
        public required string? UserComment { get; init; }
    }
}