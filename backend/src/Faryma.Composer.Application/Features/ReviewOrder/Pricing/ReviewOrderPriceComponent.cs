namespace Faryma.Composer.Application.Features.ReviewOrder.Pricing
{
    /// <summary>
    /// Компонент обязательной стоимости заказа разбора
    /// </summary>
    public sealed record ReviewOrderPriceComponent
    {
        /// <summary>
        /// Тип компонента стоимости
        /// </summary>
        public required ReviewOrderPriceComponentKind Kind { get; init; }

        /// <summary>
        /// Стоимость компонента
        /// </summary>
        public required long Amount { get; init; }

        /// <summary>
        /// Длительность трека в секундах на момент расчета
        /// </summary>
        public required int? TrackDurationSeconds { get; init; }

        /// <summary>
        /// Длительность, включенная в базовую стоимость заказа, в секундах
        /// </summary>
        public required int? IncludedDurationSeconds { get; init; }

        /// <summary>
        /// Дополнительно оплачиваемая длительность в секундах
        /// </summary>
        public required int? ExtraDurationSeconds { get; init; }

        /// <summary>
        /// Стоимость одной дополнительной секунды на момент расчета
        /// </summary>
        public required long? AmountPerSecond { get; init; }
    }
}
