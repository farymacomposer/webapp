using System.ComponentModel.DataAnnotations;

namespace Faryma.Composer.Api.Features.ReviewOrder.TakeInProgress
{
    /// <summary>
    /// Запрос взятия заказа в работу
    /// </summary>
    public sealed record TakeOrderInProgressRequest
    {
        /// <summary>
        /// Id заказа разбора трека
        /// </summary>
        [Range(1, long.MaxValue, ErrorMessage = "Id заказа должен быть больше нуля")]
        public required long ReviewOrderId { get; init; }
    }
}
