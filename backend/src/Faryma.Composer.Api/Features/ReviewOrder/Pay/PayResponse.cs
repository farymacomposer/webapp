using System.ComponentModel.DataAnnotations;
using Faryma.Composer.Api.SharedDto;

namespace Faryma.Composer.Api.Features.ReviewOrder.Pay
{
    /// <summary>
    /// Ответ на запрос оплаты заказа разбора трека
    /// </summary>
    public sealed record PayResponse
    {
        /// <summary>
        /// Заказ разбора трека
        /// </summary>
        [Required]
        public required ReviewOrderDto ReviewOrder { get; init; }
    }
}
