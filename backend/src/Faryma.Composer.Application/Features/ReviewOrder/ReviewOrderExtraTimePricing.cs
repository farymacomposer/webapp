namespace Faryma.Composer.Application.Features.ReviewOrder
{
    /// <summary>
    /// Расчет стоимости дополнительной длительности трека в заказе разбора
    /// </summary>
    public sealed record ReviewOrderExtraTimePricing
    {
        /// <summary>
        /// Длительность трека в секундах на момент расчета
        /// </summary>
        public required int TrackDurationSeconds { get; init; }

        /// <summary>
        /// Длительность, включенная в базовую стоимость заказа, в секундах
        /// </summary>
        public required int IncludedDurationSeconds { get; init; }

        /// <summary>
        /// Дополнительно оплачиваемая длительность в секундах
        /// </summary>
        public required int ExtraDurationSeconds { get; init; }

        /// <summary>
        /// Стоимость одной дополнительной секунды на момент расчета
        /// </summary>
        public required long AmountPerSecond { get; init; }

        /// <summary>
        /// Итоговая стоимость услуги
        /// </summary>
        public required long Amount { get; init; }
    }
}
