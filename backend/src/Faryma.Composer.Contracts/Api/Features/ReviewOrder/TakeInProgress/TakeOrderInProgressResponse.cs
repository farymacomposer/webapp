using Faryma.Composer.Contracts.Api.Shared.Dto;

namespace Faryma.Composer.Contracts.Api.Features.ReviewOrder.TakeInProgress
{
    /// <summary>
    /// Ответ на запрос взятия заказа в работу
    /// </summary>
    public sealed record TakeOrderInProgressResponse
    {
        /// <summary>
        /// Заказ разбора трека
        /// </summary>
        public required ReviewOrderDto ReviewOrder { get; init; }
    }
}