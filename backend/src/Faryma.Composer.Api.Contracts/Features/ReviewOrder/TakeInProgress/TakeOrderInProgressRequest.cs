namespace Faryma.Composer.Api.Contracts.Features.ReviewOrder.TakeInProgress
{
    /// <summary>
    /// Запрос взятия заказа в работу
    /// </summary>
    public sealed record TakeOrderInProgressRequest
    {
        /// <summary>
        /// Id заказа разбора трека
        /// </summary>
        public required long ReviewOrderId { get; init; }
    }
}
