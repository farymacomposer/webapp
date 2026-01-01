using System.ComponentModel.DataAnnotations.Schema;
using Faryma.Composer.Contracts.Infrastructure.Entities.Abstractions;

namespace Faryma.Composer.Contracts.Infrastructure.Entities
{
    /// <summary>
    /// Настройки приложения
    /// </summary>
    [Table("app_settings")]
    public sealed class AppSettingsEntity : BaseEntity
    {
        /// <summary>
        /// Номинальная стоимость заказа
        /// </summary>
        public required int ReviewOrderNominalAmount { get; set; }
    }
}