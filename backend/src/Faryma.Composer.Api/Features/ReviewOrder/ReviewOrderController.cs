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
using Faryma.Composer.Api.Contracts.Features.ReviewOrder.TakeInProgress;
using Faryma.Composer.Api.Contracts.Features.ReviewOrder.Unfreeze;
using Faryma.Composer.Api.Contracts.Shared.Dto;
using Faryma.Composer.Api.Features.Auth;
using Faryma.Composer.Application.Features.ReviewOrder.AddTrackUrl;
using Faryma.Composer.Application.Features.ReviewOrder.Cancel;
using Faryma.Composer.Application.Features.ReviewOrder.Complete;
using Faryma.Composer.Application.Features.ReviewOrder.CreateCharity;
using Faryma.Composer.Application.Features.ReviewOrder.CreateDonation;
using Faryma.Composer.Application.Features.ReviewOrder.CreateFree;
using Faryma.Composer.Application.Features.ReviewOrder.CreateOutOfQueue;
using Faryma.Composer.Application.Features.ReviewOrder.Freeze;
using Faryma.Composer.Application.Features.ReviewOrder.Pay;
using Faryma.Composer.Application.Features.ReviewOrder.TakeInProgress;
using Faryma.Composer.Application.Features.ReviewOrder.Unfreeze;
using Faryma.Composer.Domain;
using Faryma.Composer.Domain.Entities.TransactionSources;
using Microsoft.AspNetCore.Mvc;
using AppMediator = Mediator.Mediator;

namespace Faryma.Composer.Api.Features.ReviewOrder
{
    /// <summary>
    /// Управление заказами разборов треков
    /// </summary>
    [ApiController]
    [Route("api/[controller]/[action]")]
    [Produces("application/json")]
    public sealed class ReviewOrderController(AppMediator mediator) : ControllerBase
    {
        /// <summary>
        /// Создает внеочередной заказ
        /// </summary>
        [HttpPost]
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
                ReviewOrder = ReviewOrderDto.Map(order)
            });
        }

        /// <summary>
        /// Создает донатный заказ
        /// </summary>
        [HttpPost]
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
                ReviewOrder = ReviewOrderDto.Map(order)
            });
        }

        /// <summary>
        /// Создает бесплатный заказ
        /// </summary>
        [HttpPost]
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
                ReviewOrder = ReviewOrderDto.Map(order)
            });
        }

        /// <summary>
        /// Создает благотворительный заказ
        /// </summary>
        [HttpPost]
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
                ReviewOrder = ReviewOrderDto.Map(order)
            });
        }

        /// <summary>
        /// Оплачивает заказ
        /// </summary>
        [HttpPost]
        [AuthorizeAdmins]
        [Idempotent]
        public async Task<ActionResult<PayReviewOrderResponse>> PayReviewOrder(
            [FromHeader(Name = Globals.IdempotencyKey)] Guid idempotencyKey,
            [FromBody] PayReviewOrderRequest request,
            CancellationToken ct)
        {
            Guid userId = User.GetUserId();

            ReviewOrderEntity order = await mediator.Send(new PayCommand
            {
                ReviewOrderId = request.ReviewOrderId,
                Nickname = request.Nickname.Trim(),
                PaymentAmount = request.PaymentAmount,
                TopUpProvider = request.TopUpProvider,
                CreatedByUserId = userId,
            }, ct);

            return Ok(new PayReviewOrderResponse
            {
                ReviewOrder = ReviewOrderDto.Map(order)
            });
        }

        /// <summary>
        /// Добавляет или изменяет ссылку на трек
        /// </summary>
        [HttpPost]
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
                ReviewOrder = ReviewOrderDto.Map(order)
            });
        }

        /// <summary>
        /// Взятие заказа в работу
        /// </summary>
        [HttpPost]
        [AuthorizeAdmins]
        public async Task<ActionResult<TakeOrderInProgressResponse>> TakeOrderInProgress(TakeOrderInProgressRequest request, CancellationToken ct)
        {
            ReviewOrderEntity order = await mediator.Send(new TakeInProgressCommand
            {
                ReviewOrderId = request.ReviewOrderId
            }, ct);

            return Ok(new TakeOrderInProgressResponse
            {
                ReviewOrder = ReviewOrderDto.Map(order)
            });
        }

        /// <summary>
        /// Выполнение заказа
        /// </summary>
        [HttpPost]
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
                ReviewOrder = ReviewOrderDto.Map(order),
                ReviewId = order.Review!.Id,
            });
        }

        /// <summary>
        /// Замораживает заказ
        /// </summary>
        [HttpPost]
        [AuthorizeAdmins]
        public async Task<ActionResult<FreezeReviewOrderResponse>> FreezeReviewOrder(FreezeReviewOrderRequest request, CancellationToken ct)
        {
            ReviewOrderEntity order = await mediator.Send(new FreezeCommand
            {
                ReviewOrderId = request.ReviewOrderId
            }, ct);

            return Ok(new FreezeReviewOrderResponse
            {
                ReviewOrder = ReviewOrderDto.Map(order)
            });
        }

        /// <summary>
        /// Размораживает заказ
        /// </summary>
        [HttpPost]
        [AuthorizeAdmins]
        public async Task<ActionResult<UnfreezeReviewOrderResponse>> UnfreezeReviewOrder(UnfreezeReviewOrderRequest request, CancellationToken ct)
        {
            ReviewOrderEntity order = await mediator.Send(new UnfreezeCommand
            {
                ReviewOrderId = request.ReviewOrderId
            }, ct);

            return Ok(new UnfreezeReviewOrderResponse
            {
                ReviewOrder = ReviewOrderDto.Map(order)
            });
        }

        /// <summary>
        /// Отменяет заказ
        /// </summary>
        [HttpPost]
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
                ReviewOrder = ReviewOrderDto.Map(order)
            });
        }
    }
}
