namespace Faryma.Composer.Core.Features.AppSettings
{
    /// <summary>
    /// Настройки приложения
    /// </summary>
    public sealed record AppSettingsModel
    {
        /// <summary>
        /// Номинальная стоимость заказа (для бесплатных или минималка для платных)
        /// </summary>
        public required int ReviewOrderNominalAmount { get; init; }
    }
}