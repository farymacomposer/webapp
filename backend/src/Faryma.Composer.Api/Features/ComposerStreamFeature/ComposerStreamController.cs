using Faryma.Composer.Api.Auth;
using Faryma.Composer.Api.Features.ComposerStreamFeature.Create;
using Faryma.Composer.Api.Features.ComposerStreamFeature.Find;
using Faryma.Composer.Core.Features.ComposerStreamFeature;
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
        /// Создает стрим
        /// </summary>
        [HttpPost(nameof(CreateComposerStream))]
        [AuthorizeComposer]
        public async Task<ActionResult<CreateComposerStreamResponse>> CreateComposerStream(CreateComposerStreamRequest request)
        {
            ComposerStream item = await composerStreamService.Create(request.EventDate, request.Type);

            return Ok(new CreateComposerStreamResponse { ComposerStream = Mapper.Map(item) });
        }

        /// <summary>
        /// Возвращает список стримов
        /// </summary>
        [HttpGet(nameof(FindComposerStream))]
        public async Task<ActionResult<FindComposerStreamResponse>> FindComposerStream([FromQuery] FindComposerStreamRequest request)
        {
            IReadOnlyCollection<ComposerStream> items = await composerStreamService.Find(request.DateFrom, request.DateTo);

            return Ok(new FindComposerStreamResponse { Items = items.Select(Mapper.Map) });
        }
    }
}