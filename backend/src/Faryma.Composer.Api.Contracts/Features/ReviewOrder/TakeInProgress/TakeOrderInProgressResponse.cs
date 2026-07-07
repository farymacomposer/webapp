using System.ComponentModel.DataAnnotations;
using Faryma.Composer.Api.Contracts.Shared.Dto;

namespace Faryma.Composer.Api.Contracts.Features.ReviewOrder.TakeInProgress
{
    /// <summary>
    /// Ответ на запрос взятия заказа в работу
    /// </summary>
    public sealed record TakeOrderInProgressResponse
    {
        /// <summary>
        /// Заказ разбора трека
        /// </summary>
        [Required]
        public required ReviewOrderDto ReviewOrder { get; init; }
    }
}
