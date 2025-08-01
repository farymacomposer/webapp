using Faryma.Composer.Api.Auth;
using Faryma.Composer.Api.Features.ReviewOrderFeature.Cancel;
using Faryma.Composer.Api.Features.ReviewOrderFeature.Create;
using Faryma.Composer.Api.Features.ReviewOrderFeature.Freeze;
using Faryma.Composer.Api.Features.ReviewOrderFeature.StartReview;
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
        private static readonly TimeSpan _idempotencyKeyExpiration = TimeSpan.FromHours(1);

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

            ReviewOrder order = await reviewOrderService.Create(request.Map());

            cache.Set(key, order.Id, _idempotencyKeyExpiration);

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

            Transaction transaction = await reviewOrderService.Up(request.Map());

            cache.Set(key, transaction.Id, _idempotencyKeyExpiration);

            return Ok(new UpReviewOrderResponse { PaymentTransactionId = transaction.Id });
        }

        /// <summary>
        /// Отменяет заказ разбора трека
        /// </summary>
        /// <param name="idempotencyKey">Ключ идемпотентности</param>
        /// <param name="request">Запрос отмены заказа</param>
        [HttpPost(nameof(CancelReviewOrder))]
        [AuthorizeAdmins]
        public async Task<ActionResult<CancelReviewOrderResponse>> CancelReviewOrder(
            [FromHeader(Name = "Idempotency-Key")] Guid idempotencyKey,
            [FromBody] CancelReviewOrderRequest request)
        {
            if (idempotencyKey == Guid.Empty)
            {
                return BadRequest("Требуется заголовок Idempotency-Key");
            }

            string key = $"{nameof(CancelReviewOrder)}:{idempotencyKey}";
            if (cache.TryGetValue(key, out long id))
            {
                return Ok(new CancelReviewOrderResponse { ReviewOrderId = id });
            }

            await reviewOrderService.Cancel(request.Map());

            cache.Set(key, request.ReviewOrderId, _idempotencyKeyExpiration);

            return Ok(new CancelReviewOrderResponse { ReviewOrderId = request.ReviewOrderId });
        }

        /// <summary>
        /// Замораживает заказ разбора трека
        /// </summary>
        /// <param name="idempotencyKey">Ключ идемпотентности</param>
        /// <param name="request">Запрос заморозки заказа</param>
        [HttpPost(nameof(FreezeReviewOrder))]
        [AuthorizeAdmins]
        public async Task<ActionResult<FreezeReviewOrderResponse>> FreezeReviewOrder(
            [FromHeader(Name = "Idempotency-Key")] Guid idempotencyKey,
            [FromBody] FreezeReviewOrderRequest request)
        {
            if (idempotencyKey == Guid.Empty)
            {
                return BadRequest("Требуется заголовок Idempotency-Key");
            }

            string key = $"{nameof(FreezeReviewOrder)}:{idempotencyKey}";
            if (cache.TryGetValue(key, out long id))
            {
                return Ok(new FreezeReviewOrderResponse { ReviewOrderId = id });
            }

            await reviewOrderService.Freeze(request.Map());

            cache.Set(key, request.ReviewOrderId, _idempotencyKeyExpiration);

            return Ok(new FreezeReviewOrderResponse { ReviewOrderId = request.ReviewOrderId });
        }

        /// <summary>
        /// Начинает разбор трека
        /// </summary>
        /// <param name="idempotencyKey">Ключ идемпотентности</param>
        /// <param name="request">Запрос начала разбора</param>
        [HttpPost(nameof(StartReviewOrder))]
        [AuthorizeAdmins]
        public async Task<ActionResult<StartReviewOrderResponse>> StartReviewOrder(
            [FromHeader(Name = "Idempotency-Key")] Guid idempotencyKey,
            [FromBody] StartReviewOrderRequest request)
        {
            if (idempotencyKey == Guid.Empty)
            {
                return BadRequest("Требуется заголовок Idempotency-Key");
            }

            string key = $"{nameof(StartReviewOrder)}:{idempotencyKey}";
            if (cache.TryGetValue(key, out long id))
            {
                return Ok(new StartReviewOrderResponse { ReviewOrderId = id });
            }

            await reviewOrderService.StartReview(request.Map());

            cache.Set(key, request.ReviewOrderId, _idempotencyKeyExpiration);

            return Ok(new StartReviewOrderResponse { ReviewOrderId = request.ReviewOrderId });
        }
    }
}