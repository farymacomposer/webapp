namespace Faryma.Composer.Application.Features.ReviewOrder.Pricing
{
    /// <summary>
    /// Расчет стоимости и покрытия заказа разбора
    /// </summary>
    public sealed record ReviewOrderPricing
    {
        /// <summary>
        /// Компоненты обязательной стоимости заказа
        /// </summary>
        public required IReadOnlyList<ReviewOrderPriceComponent> PriceComponents { get; init; }

        /// <summary>
        /// Обязательная стоимость, которую нужно покрыть для готовности заказа
        /// </summary>
        public required long RequiredAmount { get; init; }

        /// <summary>
        /// Сумма покрытия обязательной стоимости
        /// </summary>
        public required long CoveredAmount { get; init; }

        /// <summary>
        /// Сумма денежных платежей по заказу
        /// </summary>
        public required long PaidAmount { get; init; }

        /// <summary>
        /// Денежная сумма, которая влияет на донатный приоритет
        /// </summary>
        public required long PaidPriorityAmount { get; init; }

        /// <summary>
        /// Обязательная стоимость заказа покрыта полностью
        /// </summary>
        public bool IsRequiredCovered => CoveredAmount >= RequiredAmount;
    }
}
