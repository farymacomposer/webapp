using Faryma.Composer.Domain.Entities;

namespace Faryma.Composer.Api.Features.AppSettings
{
    /// <summary>
    /// Настройки приложения
    /// </summary>
    public sealed record AppSettingsDto
    {
        /// <summary>
        /// Номинальная стоимость заказа
        /// </summary>
        public required long ReviewOrderNominalPrice { get; init; }

        /// <summary>
        /// Длительность трека, включенная в номинальную стоимость заказа, в секундах
        /// </summary>
        public required int IncludedTrackDurationSeconds { get; init; }

        /// <summary>
        /// Стоимость одной дополнительной секунды трека для заказа разбора
        /// </summary>
        public required long ReviewOrderExtraTrackSecondPrice { get; init; }

        /// <summary>
        /// Стоимость услуги подробного разбора заказа
        /// </summary>
        public required long ReviewOrderDetailedPrice { get; init; }

        public static AppSettingsDto Map(AppSettingsEntity item)
        {
            return new()
            {
                ReviewOrderNominalPrice = item.ReviewOrderNominalPrice,
                IncludedTrackDurationSeconds = item.IncludedTrackDurationSeconds,
                ReviewOrderExtraTrackSecondPrice = item.ReviewOrderExtraTrackSecondPrice,
                ReviewOrderDetailedPrice = item.ReviewOrderDetailedPrice,
            };
        }
    }
}
