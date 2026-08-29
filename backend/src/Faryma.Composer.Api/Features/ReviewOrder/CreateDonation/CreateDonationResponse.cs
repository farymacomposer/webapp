using System.ComponentModel.DataAnnotations;
using Faryma.Composer.Api.SharedDto;

namespace Faryma.Composer.Api.Features.ReviewOrder.CreateDonation
{
    /// <summary>
    /// Ответ на запрос создания заказа на разбор
    /// </summary>
    public sealed record CreateDonationResponse
    {
        /// <summary>
        /// Заказ разбора трека
        /// </summary>
        [Required]
        public required ReviewOrderDto ReviewOrder { get; init; }
    }
}
