namespace Faryma.Composer.Application.Features.ReviewOrder.Pricing
{
    /// <summary>
    /// Тип компонента обязательной стоимости заказа разбора
    /// </summary>
    public enum ReviewOrderPriceComponentKind
    {
        /// <summary>
        /// Базовая стоимость заказа
        /// </summary>
        Nominal = 1,

        /// <summary>
        /// Доплата за длительность трека сверх включенной длительности
        /// </summary>
        ExtraTrackDuration = 2,
    }
}
