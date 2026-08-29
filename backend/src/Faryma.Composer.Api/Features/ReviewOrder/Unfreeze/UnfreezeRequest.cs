using System.ComponentModel.DataAnnotations;

namespace Faryma.Composer.Api.Features.ReviewOrder.Unfreeze
{
    /// <summary>
    /// Запрос разморозки заказа
    /// </summary>
    public sealed record UnfreezeRequest
    {
        /// <summary>
        /// Id заказа разбора трека
        /// </summary>
        [Range(1, long.MaxValue, ErrorMessage = "Id заказа должен быть больше нуля")]
        public required long ReviewOrderId { get; init; }
    }
}
