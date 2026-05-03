using System.ComponentModel.DataAnnotations;
using Faryma.Composer.Contracts.Infrastructure.Entities;

namespace Faryma.Composer.Contracts.Api.Features.AppSettings
{
    /// <summary>
    /// Настройки приложения
    /// </summary>
    public sealed record AppSettingsDto
    {
        /// <summary>
        /// Номинальная стоимость заказа
        /// </summary>
        [Range(0, 10_000)]
        public required long ReviewOrderNominalAmount { get; init; }

        /// <summary>
        /// Стоимость одной дополнительной секунды трека для заказа разбора
        /// </summary>
        [Range(0, 10_000)]
        public required long ReviewOrderExtraTimeAmountPerSecond { get; init; }

        /// <summary>
        /// Стоимость услуги подробного разбора заказа
        /// </summary>
        [Range(0, 10_000)]
        public required long ReviewOrderDetailedReviewAmount { get; init; }

        public static AppSettingsDto Map(AppSettingsEntity item)
        {
            return new()
            {
                ReviewOrderNominalAmount = item.ReviewOrderNominalAmount,
                ReviewOrderExtraTimeAmountPerSecond = item.ReviewOrderExtraTimeAmountPerSecond,
                ReviewOrderDetailedReviewAmount = item.ReviewOrderDetailedReviewAmount,
            };
        }
    }
}
