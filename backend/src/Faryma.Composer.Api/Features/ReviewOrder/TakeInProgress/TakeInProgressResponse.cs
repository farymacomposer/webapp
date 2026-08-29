using System.ComponentModel.DataAnnotations;
using Faryma.Composer.Api.SharedDto;

namespace Faryma.Composer.Api.Features.ReviewOrder.TakeInProgress
{
    /// <summary>
    /// Ответ на запрос взятия заказа в работу
    /// </summary>
    public sealed record TakeInProgressResponse
    {
        /// <summary>
        /// Заказ разбора трека
        /// </summary>
        [Required]
        public required ReviewOrderDto ReviewOrder { get; init; }
    }
}
