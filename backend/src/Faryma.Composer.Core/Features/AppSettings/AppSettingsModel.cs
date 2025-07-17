namespace Faryma.Composer.Core.Features.AppSettings
{
    /// <summary>
    /// Настройки приложения
    /// </summary>
    public sealed class AppSettingsModel
    {
        /// <summary>
        /// Номинальная стоимость заказа (для бесплатных)
        /// </summary>
        public required int ReviewOrderNominalAmount { get; init; }
    }
}