using Faryma.Composer.Infrastructure.Abstractions;

namespace Faryma.Composer.Infrastructure.Entities
{
    /// <summary>
    /// Настройки приложения
    /// </summary>
    public sealed class AppSettingsEntity : BaseEntity
    {
        /// <summary>
        /// Номинальная стоимость заказа (для бесплатных или минималка для платных)
        /// </summary>
        public required int ReviewOrderNominalAmount { get; set; }
    }
}