using Faryma.Composer.Api.Common.Extensions;
using Faryma.Composer.Api.Contracts.Features.ComposerStream.Cancel;
using Faryma.Composer.Api.Contracts.Features.ComposerStream.Complete;
using Faryma.Composer.Api.Contracts.Features.ComposerStream.Create;
using Faryma.Composer.Api.Contracts.Features.ComposerStream.Find;
using Faryma.Composer.Api.Contracts.Features.ComposerStream.FindLiveAndPlanned;
using Faryma.Composer.Api.Contracts.Features.ComposerStream.Start;
using Faryma.Composer.Api.Contracts.Shared.Dto;
using Faryma.Composer.Api.Features.Auth;
using Faryma.Composer.Application.Features.ComposerStream;
using Faryma.Composer.Application.Features.ComposerStream.Commands;
using Faryma.Composer.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Faryma.Composer.Api.Features.ComposerStream
{
    /// <summary>
    /// Управление стримами
    /// </summary>
    [ApiController]
    [Route("api/composer-streams")]
    [Produces("application/json")]
    public sealed class ComposerStreamController(
        ComposerStreamService composerStreamService) : ControllerBase
    {
        /// <summary>
        /// Возвращает список стримов
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<FindStreamsResponse>> FindStreams([FromQuery] FindStreamsRequest request, CancellationToken ct)
        {
            List<ComposerStreamEntity> streams = await composerStreamService.Find(request.DateFrom, request.DateTo, ct);

            return Ok(new FindStreamsResponse
            {
                Streams = streams.Select(ComposerStreamDto.Map)
            });
        }

        /// <summary>
        /// Возвращает текущий и запланированные стримы
        /// </summary>
        [HttpGet("live-and-planned")]
        public async Task<ActionResult<FindLiveAndPlannedStreamsResponse>> FindLiveAndPlanned(CancellationToken ct)
        {
            List<ComposerStreamEntity> streams = await composerStreamService.FindLiveAndPlanned(ct);

            return Ok(new FindLiveAndPlannedStreamsResponse
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

            ComposerStreamEntity stream = await composerStreamService.Create(new CreateCommand
            {
                EventDate = request.EventDate,
                Type = request.Type,
                CreatedByUserId = userId
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
            ComposerStreamEntity stream = await composerStreamService.Start(request.ComposerStreamId, ct);

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
            ComposerStreamEntity stream = await composerStreamService.Complete(request.ComposerStreamId, ct);

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
            ComposerStreamEntity stream = await composerStreamService.Cancel(request.ComposerStreamId, ct);

            return Ok(new CancelStreamResponse
            {
                ComposerStream = ComposerStreamDto.Map(stream)
            });
        }
    }
}
