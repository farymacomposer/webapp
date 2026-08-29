using Faryma.Composer.Api.Common.Extensions;
using Faryma.Composer.Api.Contracts.Features.ComposerStream.Cancel;
using Faryma.Composer.Api.Contracts.Features.ComposerStream.Complete;
using Faryma.Composer.Api.Contracts.Features.ComposerStream.Create;
using Faryma.Composer.Api.Contracts.Features.ComposerStream.Find;
using Faryma.Composer.Api.Contracts.Features.ComposerStream.FindLiveAndPlanned;
using Faryma.Composer.Api.Contracts.Features.ComposerStream.Start;
using Faryma.Composer.Api.Contracts.Shared.Dto;
using Faryma.Composer.Api.Features.Auth;
using Faryma.Composer.Application.Features.ComposerStream.Cancel;
using Faryma.Composer.Application.Features.ComposerStream.Complete;
using Faryma.Composer.Application.Features.ComposerStream.Create;
using Faryma.Composer.Application.Features.ComposerStream.Find;
using Faryma.Composer.Application.Features.ComposerStream.FindLiveAndPlanned;
using Faryma.Composer.Application.Features.ComposerStream.Start;
using Faryma.Composer.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using AppMediator = Mediator.Mediator;

namespace Faryma.Composer.Api.Features.ComposerStream
{
    /// <summary>
    /// Управление стримами
    /// </summary>
    [ApiController]
    [Route("api/composer-streams")]
    [Produces("application/json")]
    public sealed class ComposerStreamController(AppMediator mediator) : ControllerBase
    {
        /// <summary>
        /// Возвращает список стримов
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<FindStreamsResponse>> FindStreams([FromQuery] FindStreamsRequest request, CancellationToken ct)
        {
            IReadOnlyCollection<ComposerStreamEntity> streams = await mediator.Send(new FindQuery
            {
                DateFrom = request.DateFrom,
                DateTo = request.DateTo,
            }, ct);

            return Ok(new FindStreamsResponse
            {
                Streams = streams.Select(ComposerStreamDto.Map)
            });
        }

        /// <summary>
        /// Возвращает текущий и запланированные стримы
        /// </summary>
        [HttpGet("live-and-planned")]
        public async Task<ActionResult<FindLiveAndPlannedResponse>> FindLiveAndPlanned(CancellationToken ct)
        {
            IReadOnlyCollection<ComposerStreamEntity> streams = await mediator.Send(new FindLiveAndPlannedQuery(), ct);

            return Ok(new FindLiveAndPlannedResponse
            {
                Streams = streams.Select(ComposerStreamDto.Map)
            });
        }

        /// <summary>
        /// Создает стрим
        /// </summary>
        [HttpPost]
        [AuthorizeComposer]
        public async Task<ActionResult<CreateStreamResponse>> CreateStream(CreateStreamRequest request, CancellationToken ct)
        {
            Guid userId = User.GetUserId();

            ComposerStreamEntity stream = await mediator.Send(new CreateCommand
            {
                EventDate = request.EventDate,
                Type = request.Type,
                CreatedByUserId = userId,
            }, ct);

            return Ok(new CreateStreamResponse
            {
                ComposerStream = ComposerStreamDto.Map(stream)
            });
        }

        /// <summary>
        /// Запускает стрим
        /// </summary>
        [HttpPost("start")]
        [AuthorizeComposer]
        public async Task<ActionResult<StartStreamResponse>> StartStream(StartStreamRequest request, CancellationToken ct)
        {
            ComposerStreamEntity stream = await mediator.Send(new StartCommand
            {
                ComposerStreamId = request.ComposerStreamId
            }, ct);

            return Ok(new StartStreamResponse
            {
                ComposerStream = ComposerStreamDto.Map(stream)
            });
        }

        /// <summary>
        /// Завершает стрим
        /// </summary>
        [HttpPost("complete")]
        [AuthorizeComposer]
        public async Task<ActionResult<CompleteStreamResponse>> CompleteStream(CompleteStreamRequest request, CancellationToken ct)
        {
            ComposerStreamEntity stream = await mediator.Send(new CompleteCommand
            {
                ComposerStreamId = request.ComposerStreamId
            }, ct);

            return Ok(new CompleteStreamResponse
            {
                ComposerStream = ComposerStreamDto.Map(stream)
            });
        }

        /// <summary>
        /// Отменяет стрим
        /// </summary>
        [HttpPost("cancel")]
        [AuthorizeComposer]
        public async Task<ActionResult<CancelStreamResponse>> CancelStream(CancelStreamRequest request, CancellationToken ct)
        {
            ComposerStreamEntity stream = await mediator.Send(new CancelCommand
            {
                ComposerStreamId = request.ComposerStreamId
            }, ct);

            return Ok(new CancelStreamResponse
            {
                ComposerStream = ComposerStreamDto.Map(stream)
            });
        }
    }
}
