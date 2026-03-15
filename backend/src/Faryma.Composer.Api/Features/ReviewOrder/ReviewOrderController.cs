using Faryma.Composer.Api.Auth;
using Faryma.Composer.Api.Extensions;
using Faryma.Composer.Application.Features.ReviewOrder;
using Faryma.Composer.Contracts.Api;
using Faryma.Composer.Contracts.Api.Features.ReviewOrder.AddTrackUrl;
using Faryma.Composer.Contracts.Api.Features.ReviewOrder.Cancel;
using Faryma.Composer.Contracts.Api.Features.ReviewOrder.Complete;
using Faryma.Composer.Contracts.Api.Features.ReviewOrder.Create;
using Faryma.Composer.Contracts.Api.Features.ReviewOrder.Freeze;
using Faryma.Composer.Contracts.Api.Features.ReviewOrder.MoveUp;
using Faryma.Composer.Contracts.Api.Features.ReviewOrder.TakeInProgress;
using Faryma.Composer.Contracts.Api.Features.ReviewOrder.Unfreeze;
using Faryma.Composer.Contracts.Api.Shared.Dto;
using Faryma.Composer.Contracts.Application.Features.ReviewOrder.Commands;
using Faryma.Composer.Contracts.Infrastructure.Entities;
using Faryma.Composer.Contracts.Infrastructure.Entities.TransactionSources;
using Faryma.Composer.Contracts.Infrastructure.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Faryma.Composer.Api.Features.ReviewOrder
{
    /// <summary>
    /// Управление заказами разборов треков
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public sealed class ReviewOrderController(
        ReviewOrderService reviewOrderService) : ControllerBase
    {
        /// <summary>
        /// Создает заказ
        /// </summary>
        /// <param name="idempotencyKey">Ключ идемпотентности</param>
        /// <param name="request">Запрос создания заказа</param>
        /// <param name="ct">Токен отмены</param>
        [HttpPost(nameof(CreateReviewOrder))]
        [AuthorizeAdmins]
        [Idempotent]
        public async Task<ActionResult<CreateReviewOrderResponse>> CreateReviewOrder(
            [FromHeader(Name = Globals.IdempotencyKey)] Guid idempotencyKey,
            [FromBody] CreateReviewOrderRequest request,
            CancellationToken ct)
        {
            _ = idempotencyKey; // Используется фильтром
            Guid userId = User.GetUserId();
            DateTime now = DateTime.UtcNow;

            ReviewOrderEntity order = request.OrderType switch
            {
                ReviewOrderType.OutOfQueue => await reviewOrderService.CreateOutOfQueue(new CreateOutOfQueueOrderCommand
                {
                    Nickname = request.Nickname,
                    TrackUrl = request.TrackUrl,
                    UserComment = request.UserComment,
                    CreatedByUserId = userId,
                }, now, ct),
                ReviewOrderType.Donation => await reviewOrderService.CreateDonation(new CreateDonationOrderCommand
                {
                    Nickname = request.Nickname,
                    TrackUrl = request.TrackUrl,
                    UserComment = request.UserComment,
                    PaymentAmount = request.PaymentAmount!.Value,
                    TopUpProvider = request.TopUpProvider!.Value,
                    CreatedByUserId = userId,
                }, now, ct),
                ReviewOrderType.Free => await reviewOrderService.CreateFree(new CreateFreeOrderCommand
                {
                    Nickname = request.Nickname,
                    TrackUrl = request.TrackUrl,
                    UserComment = request.UserComment,
                    CreatedByUserId = userId,
                }, now, ct),
                ReviewOrderType.Charity => await reviewOrderService.CreateCharity(new CreateCharityOrderCommand
                {
                    Nickname = request.Nickname,
                    TrackUrl = request.TrackUrl,
                    UserComment = request.UserComment,
                    CreatedByUserId = userId,
                }, now, ct),
                _ => throw new NotSupportedException("Неподдерживаемый тип заказа"),
            };

            return Ok(new CreateReviewOrderResponse
            {
                ReviewOrder = ReviewOrderDto.Map(order)
            });
        }

        /// <summary>
        /// Поднимает заказ в очереди
        /// </summary>
        /// <param name="idempotencyKey">Ключ идемпотентности</param>
        /// <param name="request">Запрос поднятия заказа в очереди</param>
        /// <param name="ct">Токен отмены</param>
        [HttpPost(nameof(MoveUpReviewOrder))]
        [AuthorizeAdmins]
        [Idempotent]
        public async Task<ActionResult<MoveUpReviewOrderResponse>> MoveUpReviewOrder(
            [FromHeader(Name = Globals.IdempotencyKey)] Guid idempotencyKey,
            [FromBody] MoveUpReviewOrderRequest request,
            CancellationToken ct)
        {
            _ = idempotencyKey; // Используется фильтром
            Guid userId = User.GetUserId();
            DateTime now = DateTime.UtcNow;

            TransactionEntity transaction = await reviewOrderService.MoveUp(new MoveUpCommand
            {
                ReviewOrderId = request.ReviewOrderId,
                Nickname = request.Nickname.Trim(),
                PaymentAmount = request.PaymentAmount,
                TopUpProvider = request.TopUpProvider,
                CreatedByUserId = userId,
            }, now, ct);

            return Ok(new MoveUpReviewOrderResponse
            {
                ReviewOrder = ReviewOrderDto.Map((ReviewOrderEntity)transaction.TransactionSource),
                PaymentTransactionId = transaction.Id
            });
        }

        /// <summary>
        /// Добавляет или изменяет ссылку на трек
        /// </summary>
        [HttpPost(nameof(AddTrackUrl))]
        [AuthorizeAdmins]
        public async Task<ActionResult<AddTrackUrlResponse>> AddTrackUrl(AddTrackUrlRequest request, CancellationToken ct)
        {
            ReviewOrderEntity order = await reviewOrderService.AddTrackUrl(new AddTrackUrlCommand
            {
                ReviewOrderId = request.ReviewOrderId,
                TrackUrl = request.TrackUrl,
            }, ct);

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
        public async Task<ActionResult<TakeOrderInProgressResponse>> TakeOrderInProgress(TakeOrderInProgressRequest request, CancellationToken ct)
        {
            DateTime now = DateTime.UtcNow;
            ReviewOrderEntity order = await reviewOrderService.TakeInProgress(request.ReviewOrderId, now, ct);

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
        public async Task<ActionResult<CompleteReviewOrderResponse>> CompleteReviewOrder(CompleteReviewOrderRequest request, CancellationToken ct)
        {
            Guid userId = User.GetUserId();
            DateTime now = DateTime.UtcNow;

            ReviewOrderEntity order = await reviewOrderService.Complete(new CompleteCommand
            {
                ReviewOrderId = request.ReviewOrderId,
                Rating = request.Rating,
                CreatedByUserId = userId,
            }, now, ct);

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
        public async Task<ActionResult<FreezeReviewOrderResponse>> FreezeReviewOrder(FreezeReviewOrderRequest request, CancellationToken ct)
        {
            ReviewOrderEntity order = await reviewOrderService.Freeze(request.ReviewOrderId, ct);

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
        public async Task<ActionResult<UnfreezeReviewOrderResponse>> UnfreezeReviewOrder(UnfreezeReviewOrderRequest request, CancellationToken ct)
        {
            ReviewOrderEntity order = await reviewOrderService.Unfreeze(request.ReviewOrderId, ct);

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
        public async Task<ActionResult<CancelReviewOrderResponse>> CancelReviewOrder(CancelReviewOrderRequest request, CancellationToken ct)
        {
            ReviewOrderEntity order = await reviewOrderService.Cancel(request.ReviewOrderId, ct);

            return Ok(new CancelReviewOrderResponse
            {
                ReviewOrder = ReviewOrderDto.Map(order)
            });
        }
    }
}