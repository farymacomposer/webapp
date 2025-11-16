namespace Faryma.Composer.Application.Features.AppSettings
{
    /// <summary>
    /// Настройки приложения
    /// </summary>
    public sealed record AppSettingsModel
    {
        /// <summary>
        /// Номинальная стоимость заказа
        /// </summary>
        public required int ReviewOrderNominalAmount { get; init; }
    }
}