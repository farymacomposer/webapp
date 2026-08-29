#pragma warning disable RCS1163 // Unused parameter
#pragma warning disable IDE0060 // Удалите неиспользуемый параметр

using Faryma.Composer.Api.Common.Attributes;
using Faryma.Composer.Api.Common.Extensions;
using Faryma.Composer.Api.Features.Auth;
using Faryma.Composer.Api.Features.ReviewOrder.AddTrackUrl;
using Faryma.Composer.Api.Features.ReviewOrder.Cancel;
using Faryma.Composer.Api.Features.ReviewOrder.Complete;
using Faryma.Composer.Api.Features.ReviewOrder.CreateCharity;
using Faryma.Composer.Api.Features.ReviewOrder.CreateDonation;
using Faryma.Composer.Api.Features.ReviewOrder.CreateFree;
using Faryma.Composer.Api.Features.ReviewOrder.CreateOutOfQueue;
using Faryma.Composer.Api.Features.ReviewOrder.Freeze;
using Faryma.Composer.Api.Features.ReviewOrder.Pay;
using Faryma.Composer.Api.Features.ReviewOrder.TakeInProgress;
using Faryma.Composer.Api.Features.ReviewOrder.Unfreeze;
using Faryma.Composer.Api.SharedDto;
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
    [Consumes("application/json")]
    public sealed class ReviewOrderController(AppMediator mediator) : ControllerBase
    {
        /// <summary>
        /// Создает внеочередной заказ
        /// </summary>
        [HttpPost]
        [AuthorizeAdmins]
        [Idempotent]
        public async Task<ActionResult<CreateOutOfQueueResponse>> CreateOutOfQueue(
            [FromHeader(Name = Globals.IdempotencyKey)] Guid idempotencyKey,
            [FromBody] CreateOutOfQueueRequest request,
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

            return Ok(new CreateOutOfQueueResponse
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
        public async Task<ActionResult<CreateDonationResponse>> CreateDonation(
            [FromHeader(Name = Globals.IdempotencyKey)] Guid idempotencyKey,
            [FromBody] CreateDonationRequest request,
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

            return Ok(new CreateDonationResponse
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
        public async Task<ActionResult<CreateFreeResponse>> CreateFree(
            [FromHeader(Name = Globals.IdempotencyKey)] Guid idempotencyKey,
            [FromBody] CreateFreeRequest request,
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

            return Ok(new CreateFreeResponse
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
        public async Task<ActionResult<CreateCharityResponse>> CreateCharity(
            [FromHeader(Name = Globals.IdempotencyKey)] Guid idempotencyKey,
            [FromBody] CreateCharityRequest request,
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

            return Ok(new CreateCharityResponse
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
        public async Task<ActionResult<PayResponse>> Pay(
            [FromHeader(Name = Globals.IdempotencyKey)] Guid idempotencyKey,
            [FromBody] PayRequest request,
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

            return Ok(new PayResponse
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
        public async Task<ActionResult<TakeInProgressResponse>> TakeInProgress(TakeInProgressRequest request, CancellationToken ct)
        {
            ReviewOrderEntity order = await mediator.Send(new TakeInProgressCommand
            {
                ReviewOrderId = request.ReviewOrderId
            }, ct);

            return Ok(new TakeInProgressResponse
            {
                ReviewOrder = ReviewOrderDto.Map(order)
            });
        }

        /// <summary>
        /// Выполнение заказа
        /// </summary>
        [HttpPost]
        [AuthorizeAdmins]
        public async Task<ActionResult<CompleteResponse>> Complete(CompleteRequest request, CancellationToken ct)
        {
            Guid userId = User.GetUserId();

            ReviewOrderEntity order = await mediator.Send(new CompleteCommand
            {
                ReviewOrderId = request.ReviewOrderId,
                Rating = request.Rating,
                CreatedByUserId = userId,
            }, ct);

            return Ok(new CompleteResponse
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
        public async Task<ActionResult<FreezeResponse>> Freeze(FreezeRequest request, CancellationToken ct)
        {
            ReviewOrderEntity order = await mediator.Send(new FreezeCommand
            {
                ReviewOrderId = request.ReviewOrderId
            }, ct);

            return Ok(new FreezeResponse
            {
                ReviewOrder = ReviewOrderDto.Map(order)
            });
        }

        /// <summary>
        /// Размораживает заказ
        /// </summary>
        [HttpPost]
        [AuthorizeAdmins]
        public async Task<ActionResult<UnfreezeResponse>> Unfreeze(UnfreezeRequest request, CancellationToken ct)
        {
            ReviewOrderEntity order = await mediator.Send(new UnfreezeCommand
            {
                ReviewOrderId = request.ReviewOrderId
            }, ct);

            return Ok(new UnfreezeResponse
            {
                ReviewOrder = ReviewOrderDto.Map(order)
            });
        }

        /// <summary>
        /// Отменяет заказ
        /// </summary>
        [HttpPost]
        [AuthorizeAdmins]
        public async Task<ActionResult<CancelResponse>> Cancel(CancelRequest request, CancellationToken ct)
        {
            ReviewOrderEntity order = await mediator.Send(new CancelCommand
            {
                ReviewOrderId = request.ReviewOrderId,
                CancelReason = request.CancelReason,
            }, ct);

            return Ok(new CancelResponse
            {
                ReviewOrder = ReviewOrderDto.Map(order)
            });
        }
    }
}
