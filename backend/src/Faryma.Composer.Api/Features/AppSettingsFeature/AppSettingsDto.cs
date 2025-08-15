using System.ComponentModel.DataAnnotations;
using Faryma.Composer.Infrastructure.Entities;

namespace Faryma.Composer.Api.Features.AppSettingsFeature
{
    /// <summary>
    /// Настройки приложения
    /// </summary>
    public sealed record AppSettingsDto
    {
        /// <summary>
        /// Номинальная стоимость заказа (для бесплатных или минималка для платных)
        /// </summary>
        [Range(0, 10_000)]
        public required int ReviewOrderNominalAmount { get; set; }

        public static AppSettingsDto Map(AppSettingsEntity item)
        {
            return new()
            {
                ReviewOrderNominalAmount = item.ReviewOrderNominalAmount,
            };
        }
    }
}