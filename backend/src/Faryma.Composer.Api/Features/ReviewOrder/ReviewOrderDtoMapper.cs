using Faryma.Composer.Api.Contracts.Shared.Dto;
using Faryma.Composer.Application.Features.ReviewOrder.Pricing;
using Faryma.Composer.Domain.Entities.TransactionSources;

namespace Faryma.Composer.Api.Features.ReviewOrder
{
    public sealed class ReviewOrderDtoMapper(ReviewOrderPricingService pricingService)
    {
        public ReviewOrderDto Map(ReviewOrderEntity order)
        {
            ReviewOrderPricing pricing = pricingService.Calculate(order);

            return ReviewOrderDto.Map(
                order,
                pricing.RequiredAmount,
                pricing.CoveredAmount,
                pricing.PaidAmount,
                pricing.PaidPriorityAmount);
        }
    }
}
