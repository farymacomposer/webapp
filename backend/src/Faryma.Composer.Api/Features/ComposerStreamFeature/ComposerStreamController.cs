using Faryma.Composer.Api.Auth;
using Faryma.Composer.Api.Features.ComposerStreamFeature.Cancel;
using Faryma.Composer.Api.Features.ComposerStreamFeature.Complete;
using Faryma.Composer.Api.Features.ComposerStreamFeature.Create;
using Faryma.Composer.Api.Features.ComposerStreamFeature.Find;
using Faryma.Composer.Api.Features.ComposerStreamFeature.GetCurrentAndScheduled;
using Faryma.Composer.Api.Features.ComposerStreamFeature.Start;
using Faryma.Composer.Core.Features.ComposerStreamFeature;
using Faryma.Composer.Core.Features.ComposerStreamFeature.Commands;
using Faryma.Composer.Infrastructure.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Faryma.Composer.Api.Features.ComposerStreamFeature
{
    /// <summary>
    /// Управление стримами
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public sealed class ComposerStreamController(ComposerStreamService composerStreamService) : ControllerBase
    {
        /// <summary>
        /// Возвращает список стримов
        /// </summary>
        [HttpGet(nameof(FindStreams))]
        public async Task<ActionResult<FindComposerStreamResponse>> FindStreams([FromQuery] FindComposerStreamRequest request)
        {
            IReadOnlyCollection<ComposerStream> streams = await composerStreamService.Find(request.DateFrom, request.DateTo);

            return Ok(new FindComposerStreamResponse
            {
                Streams = streams.Select(ComposerStreamDto.Map)
            });
        }

        /// <summary>
        /// Возвращает текущий и запланированные стримы
        /// </summary>
        [HttpGet(nameof(FindCurrentAndScheduledStreams))]
        public async Task<ActionResult<FindCurrentAndScheduledStreamsResponse>> FindCurrentAndScheduledStreams()
        {
            IReadOnlyCollection<ComposerStream> streams = await composerStreamService.FindCurrentAndScheduled();

            return Ok(new FindCurrentAndScheduledStreamsResponse
            {
                Streams = streams.Select(ComposerStreamDto.Map)
            });
        }

        /// <summary>
        /// Создает стрим
        /// </summary>
        [HttpPost(nameof(CreateStream))]
        [AuthorizeComposer]
        public async Task<ActionResult<CreateComposerStreamResponse>> CreateStream(CreateComposerStreamRequest request)
        {
            ComposerStream stream = await composerStreamService.Create(new CreateCommand
            {
                EventDate = request.EventDate,
                Type = request.Type
            });

            return Ok(new CreateComposerStreamResponse
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
            ComposerStream stream = await composerStreamService.Start(new StartCommand
            {
                ComposerStreamId = request.ComposerStreamId
            });

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
            ComposerStream stream = await composerStreamService.Complete(new CompleteCommand
            {
                ComposerStreamId = request.ComposerStreamId
            });

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
            ComposerStream stream = await composerStreamService.Cancel(new CancelCommand
            {
                ComposerStreamId = request.ComposerStreamId
            });

            return Ok(new CancelStreamResponse
            {
                ComposerStream = ComposerStreamDto.Map(stream)
            });
        }
    }
}