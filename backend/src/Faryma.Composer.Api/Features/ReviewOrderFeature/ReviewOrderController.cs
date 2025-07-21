using Faryma.Composer.Api.Auth;
using Faryma.Composer.Api.Features.ReviewOrderFeature.Create;
using Faryma.Composer.Api.Features.ReviewOrderFeature.Up;
using Faryma.Composer.Core.Features.ReviewOrderFeature;
using Faryma.Composer.Infrastructure.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace Faryma.Composer.Api.Features.ReviewOrderFeature
{
    /// <summary>
    /// Управление заказами разборов треков
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public sealed class ReviewOrderController(ReviewOrderService reviewOrderService, IMemoryCache cache) : ControllerBase
    {
        /// <summary>
        /// Создает заказ разбора трека
        /// </summary>
        /// <param name="idempotencyKey">Ключ идемпотентности</param>
        /// <param name="request">Запрос создания заказа</param>
        [HttpPost(nameof(CreateReviewOrder))]
        [AuthorizeAdmins]
        public async Task<ActionResult<CreateReviewOrderResponse>> CreateReviewOrder(
            [FromHeader(Name = "Idempotency-Key")] Guid idempotencyKey,
            [FromBody] CreateReviewOrderRequest request)
        {
            if (idempotencyKey == Guid.Empty)
            {
                return BadRequest("Требуется заголовок Idempotency-Key");
            }

            string key = $"{nameof(CreateReviewOrder)}:{idempotencyKey}";
            if (cache.TryGetValue(key, out long id))
            {
                return Ok(new CreateReviewOrderResponse { ReviewOrderId = id });
            }

            ReviewOrder order = await reviewOrderService.Create(Mapper.Map(request));

            cache.Set(key, order.Id, TimeSpan.FromHours(1));

            return Ok(new CreateReviewOrderResponse { ReviewOrderId = order.Id });
        }

        /// <summary>
        /// Поднимает заказ в очереди
        /// </summary>
        /// <param name="idempotencyKey">Ключ идемпотентности</param>
        /// <param name="request">Запрос поднятия заказа в очереди</param>
        [HttpPost(nameof(UpReviewOrder))]
        [AuthorizeAdmins]
        public async Task<ActionResult<UpReviewOrderResponse>> UpReviewOrder(
            [FromHeader(Name = "Idempotency-Key")] Guid idempotencyKey,
            [FromBody] UpReviewOrderRequest request)
        {
            if (idempotencyKey == Guid.Empty)
            {
                return BadRequest("Требуется заголовок Idempotency-Key");
            }

            string key = $"{nameof(UpReviewOrder)}:{idempotencyKey}";
            if (cache.TryGetValue(key, out long id))
            {
                return Ok(new UpReviewOrderResponse { PaymentTransactionId = id });
            }

            Transaction transaction = await reviewOrderService.Up(Mapper.Map(request));

            cache.Set(key, transaction.Id, TimeSpan.FromHours(1));

            return Ok(new UpReviewOrderResponse { PaymentTransactionId = transaction.Id });
        }
    }
}