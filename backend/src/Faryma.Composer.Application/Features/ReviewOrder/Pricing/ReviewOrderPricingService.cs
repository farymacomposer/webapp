using Faryma.Composer.Application.Features.AppSettings;
using Faryma.Composer.Application.Features.ReviewOrder.Models;
using Faryma.Composer.Domain.Entities.TransactionSources;
using Faryma.Composer.Domain.Enums;

namespace Faryma.Composer.Application.Features.ReviewOrder.Pricing
{
    /// <summary>
    /// Сервис расчета стоимости и покрытия заказов разбора
    /// </summary>
    public sealed class ReviewOrderPricingService(AppSettingsService appSettingsService)
    {
        /// <summary>
        /// Рассчитывает обязательную стоимость, покрытие и денежную сумму приоритета заказа
        /// </summary>
        public ReviewOrderPricing Calculate(ReviewOrderEntity order)
        {
            IReadOnlyList<ReviewOrderPriceComponent> priceComponents = CalculateRequiredPriceComponents(
                order.Type,
                order.Price,
                order.TrackDurationSeconds);
            long paidAmount = ReviewOrderPricingCalculator.CalculateOrderPaymentAmount(order);
            long nonPaymentCoverageAmount = order.CoverageRedemption?.CoveredAmount ?? 0;

            return new()
            {
                PriceComponents = priceComponents,
                RequiredAmount = order.PayableAmount,
                CoveredAmount = paidAmount + nonPaymentCoverageAmount,
                PaidAmount = paidAmount,
                PaidPriorityAmount = ReviewOrderPricingCalculator.CalculatePaidPriorityAmount(order),
            };
        }

        /// <summary>
        /// Рассчитывает компоненты обязательной стоимости заказа
        /// </summary>
        public IReadOnlyList<ReviewOrderPriceComponent> CalculateRequiredPriceComponents(
            ReviewOrderType orderType,
            long nominalAmount,
            int? trackDurationSeconds)
        {
            return orderType switch
            {
                ReviewOrderType.Donation or ReviewOrderType.Free or ReviewOrderType.OutOfQueue => CalculatePaidReviewOrderComponents(nominalAmount, trackDurationSeconds),
                ReviewOrderType.Charity => [],
                ReviewOrderType.Custom => throw new NotSupportedException("Неподдерживаемый тип заказа"),
                _ => throw new InvalidOperationException("Неподдерживаемый тип заказа"),
            };
        }

        /// <summary>
        /// Рассчитывает стоимость дополнительной длительности трека
        /// </summary>
        public ReviewOrderExtraTimePricing CalculateExtraTimePricing(int trackDurationSeconds)
        {
            int includedTrackDurationSeconds = appSettingsService.Settings.IncludedTrackDurationSeconds;
            long reviewOrderExtraTrackSecondPrice = appSettingsService.Settings.ReviewOrderExtraTrackSecondPrice;

            int extraDurationSeconds = Math.Max(0, trackDurationSeconds - includedTrackDurationSeconds);

            return new()
            {
                TrackDurationSeconds = trackDurationSeconds,
                IncludedDurationSeconds = includedTrackDurationSeconds,
                ExtraDurationSeconds = extraDurationSeconds,
                AmountPerSecond = reviewOrderExtraTrackSecondPrice,
                Amount = extraDurationSeconds * reviewOrderExtraTrackSecondPrice,
            };
        }

        /// <summary>
        /// Рассчитывает стоимость подробного разбора
        /// </summary>
        public long CalculateDetailedReviewPaymentAmount() => appSettingsService.Settings.ReviewOrderDetailedPrice;

        private List<ReviewOrderPriceComponent> CalculatePaidReviewOrderComponents(
            long nominalAmount,
            int? trackDurationSeconds)
        {
            List<ReviewOrderPriceComponent> components =
            [
                new()
                {
                    Kind = ReviewOrderPriceComponentKind.Nominal,
                    Amount = nominalAmount,
                    TrackDurationSeconds = null,
                    IncludedDurationSeconds = null,
                    ExtraDurationSeconds = null,
                    AmountPerSecond = null,
                }
            ];

            if (trackDurationSeconds is int actualTrackDurationSeconds)
            {
                ReviewOrderExtraTimePricing extraTimePricing = CalculateExtraTimePricing(actualTrackDurationSeconds);
                if (extraTimePricing.Amount > 0)
                {
                    components.Add(new()
                    {
                        Kind = ReviewOrderPriceComponentKind.ExtraTrackDuration,
                        Amount = extraTimePricing.Amount,
                        TrackDurationSeconds = extraTimePricing.TrackDurationSeconds,
                        IncludedDurationSeconds = extraTimePricing.IncludedDurationSeconds,
                        ExtraDurationSeconds = extraTimePricing.ExtraDurationSeconds,
                        AmountPerSecond = extraTimePricing.AmountPerSecond,
                    });
                }
            }

            return components;
        }
    }
}
