using System.ComponentModel.DataAnnotations;

namespace Faryma.Composer.Api.Features.AppSettingsFeature
{
    /// <summary>
    /// Настройки приложения
    /// </summary>
    public sealed class AppSettingsDto
    {
        /// <summary>
        /// Номинальная стоимость заказа (для бесплатных разборов)
        /// </summary>
        [Range(0, 10_000)]
        public required int ReviewOrderNominalAmount { get; set; }
    }
}