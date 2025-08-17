using Faryma.Composer.Api.Auth;
using Faryma.Composer.Api.Features.CommonDto;
using Faryma.Composer.Api.Features.ReviewOrderFeature.AddTrackUrl;
using Faryma.Composer.Api.Features.ReviewOrderFeature.Cancel;
using Faryma.Composer.Api.Features.ReviewOrderFeature.Complete;
using Faryma.Composer.Api.Features.ReviewOrderFeature.Create;
using Faryma.Composer.Api.Features.ReviewOrderFeature.Freeze;
using Faryma.Composer.Api.Features.ReviewOrderFeature.TakeInProgress;
using Faryma.Composer.Api.Features.ReviewOrderFeature.Unfreeze;
using Faryma.Composer.Api.Features.ReviewOrderFeature.Up;
using Faryma.Composer.Core.Features.ReviewOrderFeature;
using Faryma.Composer.Core.Features.ReviewOrderFeature.Commands;
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
        /// Создает заказ
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

            string key = $"CreateReviewOrder:{idempotencyKey}";
            if (cache.TryGetValue(key, out CreateReviewOrderResponse? response))
            {
                return Ok(response);
            }

            ReviewOrder order = await reviewOrderService.Create(new CreateCommand
            {
                Nickname = request.Nickname.Trim(),
                OrderType = request.OrderType,
                PaymentAmount = request.PaymentAmount,
                TrackUrl = request.TrackUrl,
                UserComment = request.UserComment?.Trim(),
            });

            response = new CreateReviewOrderResponse
            {
                ReviewOrder = ReviewOrderDto.Map(order)
            };

            cache.Set(key, response, _idempotencyKeyExpiration);

            return Ok(response);
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

            string key = $"UpReviewOrder:{idempotencyKey}";
            if (cache.TryGetValue(key, out UpReviewOrderResponse? response))
            {
                return Ok(response);
            }

            Transaction transaction = await reviewOrderService.Up(new UpCommand
            {
                ReviewOrderId = request.ReviewOrderId,
                Nickname = request.Nickname.Trim(),
                PaymentAmount = request.PaymentAmount,
            });

            response = new UpReviewOrderResponse
            {
                ReviewOrder = ReviewOrderDto.Map(transaction.ReviewOrder!),
                PaymentTransactionId = transaction.Id
            };

            cache.Set(key, response, _idempotencyKeyExpiration);

            return Ok(response);
        }

        /// <summary>
        /// Добавляет или изменяет ссылку на трек
        /// </summary>
        [HttpPost(nameof(AddTrackUrl))]
        [AuthorizeAdmins]
        public async Task<ActionResult<AddTrackUrlResponse>> AddTrackUrl(AddTrackUrlRequest request)
        {
            ReviewOrder order = await reviewOrderService.AddTrackUrl(new AddTrackUrlCommand
            {
                ReviewOrderId = request.ReviewOrderId,
                TrackUrl = request.TrackUrl,
            });

            return Ok(new AddTrackUrlResponse
            {
                ReviewOrder = ReviewOrderDto.Map(order)
            });
        }

        /// <summary>
        /// Взятие заказа в работу
        /// </summary>
        [HttpPost(nameof(TakeOrderInProgress))]
        [AuthorizeAdmins]
        public async Task<ActionResult<TakeOrderInProgressResponse>> TakeOrderInProgress(TakeOrderInProgressRequest request)
        {
            ReviewOrder order = await reviewOrderService.TakeInProgress(new TakeInProgressCommand
            {
                ReviewOrderId = request.ReviewOrderId,
            });

            return Ok(new TakeOrderInProgressResponse
            {
                ReviewOrder = ReviewOrderDto.Map(order)
            });
        }

        /// <summary>
        /// Выполнение заказа
        /// </summary>
        [HttpPost(nameof(CompleteReviewOrder))]
        [AuthorizeAdmins]
        public async Task<ActionResult<CompleteReviewOrderResponse>> CompleteReviewOrder(CompleteReviewOrderRequest request)
        {
            ReviewOrder order = await reviewOrderService.Complete(new CompleteCommand
            {
                ReviewOrderId = request.ReviewOrderId,
                Rating = request.Rating,
            });

            return Ok(new CompleteReviewOrderResponse
            {
                ReviewOrder = ReviewOrderDto.Map(order),
                ReviewId = order.Review!.Id,
            });
        }

        /// <summary>
        /// Замораживает заказ
        /// </summary>
        [HttpPost(nameof(FreezeReviewOrder))]
        [AuthorizeAdmins]
        public async Task<ActionResult<FreezeReviewOrderResponse>> FreezeReviewOrder(FreezeReviewOrderRequest request)
        {
            ReviewOrder order = await reviewOrderService.Freeze(new FreezeCommand
            {
                ReviewOrderId = request.ReviewOrderId
            });

            return Ok(new FreezeReviewOrderResponse
            {
                ReviewOrder = ReviewOrderDto.Map(order)
            });
        }

        /// <summary>
        /// Размораживает заказ
        /// </summary>
        [HttpPost(nameof(UnfreezeReviewOrder))]
        [AuthorizeAdmins]
        public async Task<ActionResult<UnfreezeReviewOrderResponse>> UnfreezeReviewOrder(UnfreezeReviewOrderRequest request)
        {
            ReviewOrder order = await reviewOrderService.Unfreeze(new UnfreezeCommand
            {
                ReviewOrderId = request.ReviewOrderId
            });

            return Ok(new UnfreezeReviewOrderResponse
            {
                ReviewOrder = ReviewOrderDto.Map(order)
            });
        }

        /// <summary>
        /// Отменяет заказ
        /// </summary>
        [HttpPost(nameof(CancelReviewOrder))]
        [AuthorizeAdmins]
        public async Task<ActionResult<CancelReviewOrderResponse>> CancelReviewOrder(CancelReviewOrderRequest request)
        {
            ReviewOrder order = await reviewOrderService.Cancel(new CancelCommand
            {
                ReviewOrderId = request.ReviewOrderId
            });

            return Ok(new CancelReviewOrderResponse
            {
                ReviewOrder = ReviewOrderDto.Map(order)
            });
        }
    }
}