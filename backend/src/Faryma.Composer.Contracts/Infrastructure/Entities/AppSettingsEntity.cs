using Faryma.Composer.Contracts.Infrastructure.Entities.Abstractions;

namespace Faryma.Composer.Contracts.Infrastructure.Entities
{
    /// <summary>
    /// Настройки приложения
    /// </summary>
    public sealed class AppSettingsEntity : BaseEntity
    {
        /// <summary>
        /// Номинальная стоимость заказа
        /// </summary>
        public required long ReviewOrderNominalAmount { get; set; }

        /// <summary>
        /// Стоимость одной дополнительной секунды трека для заказа разбора
        /// </summary>
        public required long ReviewOrderExtraTimeAmountPerSecond { get; set; }

        /// <summary>
        /// Стоимость услуги подробного разбора заказа
        /// </summary>
        public required long ReviewOrderDetailedReviewAmount { get; set; }
    }
}
