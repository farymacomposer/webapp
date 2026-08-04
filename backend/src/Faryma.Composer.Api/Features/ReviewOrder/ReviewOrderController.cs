#pragma warning disable RCS1163 // Unused parameter
#pragma warning disable IDE0060 // Удалите неиспользуемый параметр

using Faryma.Composer.Api.Common.Attributes;
using Faryma.Composer.Api.Common.Extensions;
using Faryma.Composer.Api.Contracts.Features.ReviewOrder.AddTrackUrl;
using Faryma.Composer.Api.Contracts.Features.ReviewOrder.Cancel;
using Faryma.Composer.Api.Contracts.Features.ReviewOrder.Complete;
using Faryma.Composer.Api.Contracts.Features.ReviewOrder.Create;
using Faryma.Composer.Api.Contracts.Features.ReviewOrder.Freeze;
using Faryma.Composer.Api.Contracts.Features.ReviewOrder.Pay;
using Faryma.Composer.Api.Contracts.Features.ReviewOrder.PayDetailedReview;
using Faryma.Composer.Api.Contracts.Features.ReviewOrder.TakeInProgress;
using Faryma.Composer.Api.Contracts.Features.ReviewOrder.Unfreeze;
using Faryma.Composer.Api.Features.Auth;
using Faryma.Composer.Application.Features.ReviewOrder;
using Faryma.Composer.Application.Features.ReviewOrder.AddTrackUrl;
using Faryma.Composer.Application.Features.ReviewOrder.Cancel;
using Faryma.Composer.Application.Features.ReviewOrder.Complete;
using Faryma.Composer.Application.Features.ReviewOrder.CreateCharity;
using Faryma.Composer.Application.Features.ReviewOrder.CreateDonation;
using Faryma.Composer.Application.Features.ReviewOrder.CreateFree;
using Faryma.Composer.Application.Features.ReviewOrder.CreateOutOfQueue;
using Faryma.Composer.Domain;
using Faryma.Composer.Domain.Entities;
using Faryma.Composer.Domain.Entities.TransactionSources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AppMediator = Mediator.Mediator;

namespace Faryma.Composer.Api.Features.ReviewOrder
{
    /// <summary>
    /// Управление заказами разборов треков
    /// </summary>
    [ApiController]
    [Route("api/review-orders")]
    [Produces("application/json")]
    public sealed class ReviewOrderController(
        AppMediator mediator,
        ReviewOrderDtoMapper reviewOrderDtoMapper) : ControllerBase
    {
        /// <summary>
        /// Создает внеочередной заказ
        /// </summary>
        [HttpPost("create/out-of-queue")]
        [AuthorizeAdmins]
        [Idempotent]
        public async Task<ActionResult<CreateReviewOrderResponse>> CreateOutOfQueueReviewOrder(
            [FromHeader(Name = Globals.IdempotencyKey)] Guid idempotencyKey,
            [FromBody] CreateOutOfQueueReviewOrderRequest request,
            CancellationToken ct)
        {
            Guid userId = User.GetUserId();

            ReviewOrderEntity order = await mediator.Send(new CreateOutOfQueueCommand
            {
                UserNickname = request.UserNickname,
                UserComment = request.UserComment,
                TrackUrl = request.TrackUrl,
                TrackDurationSeconds = request.TrackDurationSeconds,
                CreatedByUserId = userId,
            }, ct);

            return Ok(new CreateReviewOrderResponse
            {
                ReviewOrder = reviewOrderDtoMapper.Map(order)
            });
        }

        /// <summary>
        /// Создает донатный заказ
        /// </summary>
        [HttpPost("create/donation")]
        [AuthorizeAdmins]
        [Idempotent]
        public async Task<ActionResult<CreateReviewOrderResponse>> CreateDonationReviewOrder(
            [FromHeader(Name = Globals.IdempotencyKey)] Guid idempotencyKey,
            [FromBody] CreateDonationReviewOrderRequest request,
            CancellationToken ct)
        {
            Guid userId = User.GetUserId();

            ReviewOrderEntity order = await mediator.Send(new CreateDonationCommand
            {
                UserNickname = request.UserNickname,
                UserComment = request.UserComment,
                TrackUrl = request.TrackUrl,
                TrackDurationSeconds = request.TrackDurationSeconds,
                PaymentAmount = request.PaymentAmount,
                TopUpProvider = request.TopUpProvider,
                CreatedByUserId = userId,
            }, ct);

            return Ok(new CreateReviewOrderResponse
            {
                ReviewOrder = reviewOrderDtoMapper.Map(order)
            });
        }

        /// <summary>
        /// Создает бесплатный заказ
        /// </summary>
        [HttpPost("create/free")]
        [AuthorizeAdmins]
        [Idempotent]
        public async Task<ActionResult<CreateReviewOrderResponse>> CreateFreeReviewOrder(
            [FromHeader(Name = Globals.IdempotencyKey)] Guid idempotencyKey,
            [FromBody] CreateFreeReviewOrderRequest request,
            CancellationToken ct)
        {
            Guid userId = User.GetUserId();

            ReviewOrderEntity order = await mediator.Send(new CreateFreeCommand
            {
                UserNickname = request.UserNickname,
                UserComment = request.UserComment,
                TrackUrl = request.TrackUrl,
                TrackDurationSeconds = request.TrackDurationSeconds,
                CreatedByUserId = userId,
            }, ct);

            return Ok(new CreateReviewOrderResponse
            {
                ReviewOrder = reviewOrderDtoMapper.Map(order)
            });
        }

        /// <summary>
        /// Создает заказ по существующему жетону пользователя
        /// </summary>
        [HttpPost("create/token")]
        [Authorize]
        [Idempotent]
        public async Task<ActionResult<CreateReviewOrderResponse>> CreateTokenReviewOrder(
            [FromHeader(Name = Globals.IdempotencyKey)] Guid idempotencyKey,
            [FromBody] CreateTokenReviewOrderRequest request,
            CancellationToken ct)
        {
            Guid userId = User.GetUserId();

            ReviewOrderEntity order = await mediator.Send(new CreateTokenOrderCommand
            {
                UserNickname = request.UserNickname,
                UserComment = request.UserComment,
                TrackUrl = request.TrackUrl,
                TrackDurationSeconds = request.TrackDurationSeconds,
                UserEntitlementId = request.UserEntitlementId,
                CreatedByUserId = userId,
            }, ct);

            return Ok(new CreateReviewOrderResponse
            {
                ReviewOrder = reviewOrderDtoMapper.Map(order)
            });
        }

        /// <summary>
        /// Создает благотворительный заказ
        /// </summary>
        [HttpPost("create/charity")]
        [AuthorizeAdmins]
        [Idempotent]
        public async Task<ActionResult<CreateReviewOrderResponse>> CreateCharityReviewOrder(
            [FromHeader(Name = Globals.IdempotencyKey)] Guid idempotencyKey,
            [FromBody] CreateCharityReviewOrderRequest request,
            CancellationToken ct)
        {
            Guid userId = User.GetUserId();

            ReviewOrderEntity order = await mediator.Send(new CreateCharityCommand
            {
                UserNickname = request.UserNickname,
                UserComment = request.UserComment,
                TrackUrl = request.TrackUrl,
                TrackDurationSeconds = request.TrackDurationSeconds,
                CreatedByUserId = userId,
            }, ct);

            return Ok(new CreateReviewOrderResponse
            {
                ReviewOrder = reviewOrderDtoMapper.Map(order)
            });
        }

        /// <summary>
        /// Оплачивает заказ
        /// </summary>
        [HttpPost("pay")]
        [AuthorizeAdmins]
        [Idempotent]
        public async Task<ActionResult<PayReviewOrderResponse>> PayReviewOrder(
            [FromHeader(Name = Globals.IdempotencyKey)] Guid idempotencyKey,
            [FromBody] PayReviewOrderRequest request,
            CancellationToken ct)
        {
            Guid userId = User.GetUserId();

            TransactionEntity transaction = await mediator.Send(new PayOrderCommand
            {
                ReviewOrderId = request.ReviewOrderId,
                Nickname = request.Nickname.Trim(),
                PaymentAmount = request.PaymentAmount,
                TopUpProvider = request.TopUpProvider,
                CreatedByUserId = userId,
            }, ct);

            return Ok(new PayReviewOrderResponse
            {
                ReviewOrder = reviewOrderDtoMapper.Map((ReviewOrderEntity)transaction.TransactionSource),
                PaymentTransactionId = transaction.Id
            });
        }

        /// <summary>
        /// Оплачивает подробный разбор заказа
        /// </summary>
        [HttpPost("pay-detailed-review")]
        [AuthorizeAdmins]
        [Idempotent]
        public async Task<ActionResult<PayDetailedReviewOrderResponse>> PayDetailedReview(
            [FromHeader(Name = Globals.IdempotencyKey)] Guid idempotencyKey,
            [FromBody] PayDetailedReviewOrderRequest request,
            CancellationToken ct)
        {
            Guid userId = User.GetUserId();

            PayDetailedReviewResult result = await mediator.Send(new PayDetailedReviewCommand
            {
                ReviewOrderId = request.ReviewOrderId,
                Nickname = request.Nickname.Trim(),
                TopUpProvider = request.TopUpProvider,
                UserEntitlementId = request.UserEntitlementId,
                CreatedByUserId = userId,
            }, ct);

            return Ok(new PayDetailedReviewOrderResponse
            {
                ReviewOrder = reviewOrderDtoMapper.Map(result.ReviewOrder),
                PaymentTransactionId = result.PaymentTransaction?.Id,
                UserEntitlementRedemptionId = result.UserEntitlementRedemption?.Id
            });
        }

        /// <summary>
        /// Добавляет или изменяет ссылку на трек
        /// </summary>
        [HttpPost("track-url")]
        [AuthorizeAdmins]
        public async Task<ActionResult<AddTrackUrlResponse>> AddTrackUrl(AddTrackUrlRequest request, CancellationToken ct)
        {
            ReviewOrderEntity order = await mediator.Send(new AddTrackUrlCommand
            {
                ReviewOrderId = request.ReviewOrderId,
                TrackUrl = request.TrackUrl,
                TrackDurationSeconds = request.TrackDurationSeconds,
            }, ct);

            return Ok(new AddTrackUrlResponse
            {
                ReviewOrder = reviewOrderDtoMapper.Map(order)
            });
        }

        /// <summary>
        /// Взятие заказа в работу
        /// </summary>
        [HttpPost("take-in-progress")]
        [AuthorizeAdmins]
        public async Task<ActionResult<TakeOrderInProgressResponse>> TakeOrderInProgress(TakeOrderInProgressRequest request, CancellationToken ct)
        {
            //ReviewOrderEntity order = await mediator.Send(request.ReviewOrderId, ct);

            return Ok(new TakeOrderInProgressResponse
            {
                ReviewOrder = reviewOrderDtoMapper.Map(order)
            });
        }

        /// <summary>
        /// Выполнение заказа
        /// </summary>
        [HttpPost("complete")]
        [AuthorizeAdmins]
        public async Task<ActionResult<CompleteReviewOrderResponse>> CompleteReviewOrder(CompleteReviewOrderRequest request, CancellationToken ct)
        {
            Guid userId = User.GetUserId();

            ReviewOrderEntity order = await mediator.Send(new CompleteCommand
            {
                ReviewOrderId = request.ReviewOrderId,
                Rating = request.Rating,
                CreatedByUserId = userId,
            }, ct);

            return Ok(new CompleteReviewOrderResponse
            {
                ReviewOrder = reviewOrderDtoMapper.Map(order),
                ReviewId = order.Review!.Id,
            });
        }

        /// <summary>
        /// Замораживает заказ
        /// </summary>
        [HttpPost("freeze")]
        [AuthorizeAdmins]
        public async Task<ActionResult<FreezeReviewOrderResponse>> FreezeReviewOrder(FreezeReviewOrderRequest request, CancellationToken ct)
        {
            //ReviewOrderEntity order = await mediator.Send(request.ReviewOrderId, ct);

            return Ok(new FreezeReviewOrderResponse
            {
                ReviewOrder = reviewOrderDtoMapper.Map(order)
            });
        }

        /// <summary>
        /// Размораживает заказ
        /// </summary>
        [HttpPost("unfreeze")]
        [AuthorizeAdmins]
        public async Task<ActionResult<UnfreezeReviewOrderResponse>> UnfreezeReviewOrder(UnfreezeReviewOrderRequest request, CancellationToken ct)
        {
            //ReviewOrderEntity order = await mediator.Send(request.ReviewOrderId, ct);

            return Ok(new UnfreezeReviewOrderResponse
            {
                ReviewOrder = reviewOrderDtoMapper.Map(order)
            });
        }

        /// <summary>
        /// Отменяет заказ
        /// </summary>
        [HttpPost("cancel")]
        [AuthorizeAdmins]
        public async Task<ActionResult<CancelReviewOrderResponse>> CancelReviewOrder(CancelReviewOrderRequest request, CancellationToken ct)
        {
            ReviewOrderEntity order = await mediator.Send(new CancelCommand
            {
                ReviewOrderId = request.ReviewOrderId,
                CancelReason = request.CancelReason,
            }, ct);

            return Ok(new CancelReviewOrderResponse
            {
                ReviewOrder = reviewOrderDtoMapper.Map(order)
            });
        }
    }
}
