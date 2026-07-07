using Faryma.Composer.Domain.Entities.Abstractions;

namespace Faryma.Composer.Domain.Entities
{
    /// <summary>
    /// Настройки приложения
    /// </summary>
    public sealed class AppSettingsEntity : BaseEntity
    {
        /// <summary>
        /// Номинальная стоимость заказа
        /// </summary>
        public required long ReviewOrderNominalPrice { get; set; }

        /// <summary>
        /// Длительность трека, включенная в номинальную стоимость заказа, в секундах
        /// </summary>
        public required int IncludedTrackDurationSeconds { get; set; }

        /// <summary>
        /// Стоимость одной дополнительной секунды трека для заказа разбора
        /// </summary>
        public required long ReviewOrderExtraTrackSecondPrice { get; set; }

        /// <summary>
        /// Стоимость услуги подробного разбора заказа
        /// </summary>
        public required long ReviewOrderDetailedPrice { get; set; }
    }
}
