using Faryma.Composer.Api.Features.Auth;
using Faryma.Composer.Api.Features.ComposerStream.Cancel;
using Faryma.Composer.Api.Features.ComposerStream.Complete;
using Faryma.Composer.Api.Features.ComposerStream.Create;
using Faryma.Composer.Api.Features.ComposerStream.Find;
using Faryma.Composer.Api.Features.ComposerStream.FindLiveAndPlanned;
using Faryma.Composer.Api.Features.ComposerStream.Start;
using Faryma.Composer.Api.SharedDto;
using Faryma.Composer.Application.Features.ComposerStream.Cancel;
using Faryma.Composer.Application.Features.ComposerStream.Complete;
using Faryma.Composer.Application.Features.ComposerStream.Create;
using Faryma.Composer.Application.Features.ComposerStream.Find;
using Faryma.Composer.Application.Features.ComposerStream.FindLiveAndPlanned;
using Faryma.Composer.Application.Features.ComposerStream.Start;
using Faryma.Composer.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AppMediator = Mediator.Mediator;

namespace Faryma.Composer.Api.Features.ComposerStream
{
    /// <summary>
    /// Управление стримами
    /// </summary>
    [ApiController]
    [Route("api/[controller]/[action]")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public sealed class ComposerStreamController(AppMediator mediator) : ControllerBase
    {
        /// <summary>
        /// Возвращает список стримов
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<FindResponse>> Find([FromQuery] FindRequest request, CancellationToken ct)
        {
            IReadOnlyCollection<ComposerStreamEntity> streams = await mediator.Send(new FindStreamsQuery
            {
                DateFrom = request.DateFrom,
                DateTo = request.DateTo,
            }, ct);

            return Ok(new FindResponse
            {
                Streams = streams.Select(ComposerStreamDto.Map)
            });
        }

        /// <summary>
        /// Возвращает текущий и запланированные стримы
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<FindLiveAndPlannedResponse>> FindLiveAndPlanned(CancellationToken ct)
        {
            IReadOnlyCollection<ComposerStreamEntity> streams = await mediator.Send(new FindLiveAndPlannedStreamsQuery(), ct);

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
        public async Task<ActionResult<CreateResponse>> Create(CreateRequest request, CancellationToken ct)
        {
            ComposerStreamEntity stream = await mediator.Send(new CreateCommand
            {
                EventDate = request.EventDate,
                Type = request.Type,
            }, ct);

            return Ok(new CreateResponse
            {
                ComposerStream = ComposerStreamDto.Map(stream)
            });
        }

        /// <summary>
        /// Запускает стрим
        /// </summary>
        [HttpPost]
        [AuthorizeComposer]
        public async Task<ActionResult<StartResponse>> Start(StartRequest request, CancellationToken ct)
        {
            ComposerStreamEntity stream = await mediator.Send(new StartCommand
            {
                ComposerStreamId = request.ComposerStreamId
            }, ct);

            return Ok(new StartResponse
            {
                ComposerStream = ComposerStreamDto.Map(stream)
            });
        }

        /// <summary>
        /// Завершает стрим
        /// </summary>
        [HttpPost]
        [AuthorizeComposer]
        public async Task<ActionResult<CompleteResponse>> Complete(CompleteRequest request, CancellationToken ct)
        {
            ComposerStreamEntity stream = await mediator.Send(new CompleteCommand
            {
                ComposerStreamId = request.ComposerStreamId
            }, ct);

            return Ok(new CompleteResponse
            {
                ComposerStream = ComposerStreamDto.Map(stream)
            });
        }

        /// <summary>
        /// Отменяет стрим
        /// </summary>
        [HttpPost]
        [AuthorizeComposer]
        public async Task<ActionResult<CancelResponse>> Cancel(CancelRequest request, CancellationToken ct)
        {
            ComposerStreamEntity stream = await mediator.Send(new CancelCommand
            {
                ComposerStreamId = request.ComposerStreamId
            }, ct);

            return Ok(new CancelResponse
            {
                ComposerStream = ComposerStreamDto.Map(stream)
            });
        }
    }
}
