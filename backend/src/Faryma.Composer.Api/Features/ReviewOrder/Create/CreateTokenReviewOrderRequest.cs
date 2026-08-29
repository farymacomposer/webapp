using System.ComponentModel.DataAnnotations;

namespace Faryma.Composer.Api.Features.ReviewOrder.Create
{
    /// <summary>
    /// Запрос создания заказа на разбор по существующему жетону пользователя
    /// </summary>
    public sealed record CreateTokenReviewOrderRequest : CreateReviewOrderRequestBase
    {
        /// <summary>
        /// Id жетона пользователя
        /// </summary>
        [Range(1, long.MaxValue, ErrorMessage = "Id жетона должен быть больше нуля")]
        public required long UserEntitlementId { get; init; }
    }
}
