using Faryma.Composer.Api.Features.CommonDto;

namespace Faryma.Composer.Api.Features.ReviewOrderFeature.TakeInProgress
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