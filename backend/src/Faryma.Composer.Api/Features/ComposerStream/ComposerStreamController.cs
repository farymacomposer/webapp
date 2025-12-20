using Faryma.Composer.Api.Auth;
using Faryma.Composer.Application.Features.ComposerStream;
using Faryma.Composer.Contracts.Api.Features.ComposerStream.Cancel;
using Faryma.Composer.Contracts.Api.Features.ComposerStream.Complete;
using Faryma.Composer.Contracts.Api.Features.ComposerStream.Create;
using Faryma.Composer.Contracts.Api.Features.ComposerStream.Find;
using Faryma.Composer.Contracts.Api.Features.ComposerStream.FindLiveAndPlanned;
using Faryma.Composer.Contracts.Api.Features.ComposerStream.Start;
using Faryma.Composer.Contracts.Api.Shared.Dto;
using Faryma.Composer.Contracts.Application.Features.ComposerStream.Commands;
using Faryma.Composer.Contracts.Infrastructure.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Faryma.Composer.Api.Features.ComposerStream
{
    /// <summary>
    /// Управление стримами
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public sealed class ComposerStreamController(ComposerStreamService composerStreamService) : ControllerBase
    {
        /// <summary>
        /// Возвращает список стримов
        /// </summary>
        [HttpGet(nameof(FindStreams))]
        public async Task<ActionResult<FindStreamsResponse>> FindStreams([FromQuery] FindStreamsRequest request)
        {
            List<ComposerStreamEntity> streams = await composerStreamService.Find(request.DateFrom, request.DateTo);

            return Ok(new FindStreamsResponse
            {
                Streams = streams.Select(ComposerStreamDto.Map)
            });
        }

        /// <summary>
        /// Возвращает текущий и запланированные стримы
        /// </summary>
        [HttpGet(nameof(FindLiveAndPlanned))]
        public async Task<ActionResult<FindLiveAndPlannedStreamsResponse>> FindLiveAndPlanned()
        {
            List<ComposerStreamEntity> streams = await composerStreamService.FindLiveAndPlanned();

            return Ok(new FindLiveAndPlannedStreamsResponse
            {
                Streams = streams.Select(ComposerStreamDto.Map)
            });
        }

        /// <summary>
        /// Создает стрим
        /// </summary>
        [HttpPost(nameof(CreateStream))]
        [AuthorizeComposer]
        public async Task<ActionResult<CreateStreamResponse>> CreateStream(CreateStreamRequest request)
        {
            ComposerStreamEntity stream = await composerStreamService.Create(new CreateCommand
            {
                EventDate = request.EventDate,
                Type = request.Type
            });

            return Ok(new CreateStreamResponse
            {
                ComposerStream = ComposerStreamDto.Map(stream)
            });
        }

        /// <summary>
        /// Запускает стрим
        /// </summary>
        [HttpPost(nameof(StartStream))]
        [AuthorizeComposer]
        public async Task<ActionResult<StartStreamResponse>> StartStream(StartStreamRequest request)
        {
            ComposerStreamEntity stream = await composerStreamService.Start(request.ComposerStreamId);

            return Ok(new StartStreamResponse
            {
                ComposerStream = ComposerStreamDto.Map(stream)
            });
        }

        /// <summary>
        /// Завершает стрим
        /// </summary>
        [HttpPost(nameof(CompleteStream))]
        [AuthorizeComposer]
        public async Task<ActionResult<CompleteStreamResponse>> CompleteStream(CompleteStreamRequest request)
        {
            ComposerStreamEntity stream = await composerStreamService.Complete(request.ComposerStreamId);

            return Ok(new CompleteStreamResponse
            {
                ComposerStream = ComposerStreamDto.Map(stream)
            });
        }

        /// <summary>
        /// Отменяет стрим
        /// </summary>
        [HttpPost(nameof(CancelStream))]
        [AuthorizeComposer]
        public async Task<ActionResult<CancelStreamResponse>> CancelStream(CancelStreamRequest request)
        {
            ComposerStreamEntity stream = await composerStreamService.Cancel(request.ComposerStreamId);

            return Ok(new CancelStreamResponse
            {
                ComposerStream = ComposerStreamDto.Map(stream)
            });
        }
    }
}