namespace Faryma.Composer.Contracts.Application.Features.AppSettings
{
    /// <summary>
    /// Настройки приложения
    /// </summary>
    public sealed record AppSettingsModel
    {
        /// <summary>
        /// Номинальная стоимость заказа
        /// </summary>
        public required long ReviewOrderNominalAmount { get; init; }

        /// <summary>
        /// Стоимость одной дополнительной секунды трека для заказа разбора
        /// </summary>
        public required long ReviewOrderExtraTimeAmountPerSecond { get; init; }

        /// <summary>
        /// Стоимость услуги подробного разбора заказа
        /// </summary>
        public required long ReviewOrderDetailedReviewAmount { get; init; }
    }
}
