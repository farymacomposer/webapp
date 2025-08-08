using Faryma.Composer.Core.Features.ReviewOrderFeature.Commands;

namespace Faryma.Composer.Api.Features.ReviewOrderFeature.TakeInProgress
{
    /// <summary>
    /// Запрос взятия заказа в работу
    /// </summary>
    public sealed record TakeOrderInProgressRequest
    {
        /// <summary>
        /// Id заказа разбора трека
        /// </summary>
        public required long ReviewOrderId { get; set; }

        public TakeInProgressCommand Map()
        {
            return new()
            {
                ReviewOrderId = ReviewOrderId,
            };
        }
    }
}