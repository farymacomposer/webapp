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
        public required int ReviewOrderNominalAmount { get; init; }

        public static AppSettingsDto Map(AppSettingsEntity item)
        {
            return new()
            {
                ReviewOrderNominalAmount = item.ReviewOrderNominalAmount,
            };
        }
    }
}
