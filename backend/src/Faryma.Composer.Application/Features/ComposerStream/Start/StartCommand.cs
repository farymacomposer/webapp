using Faryma.Composer.Domain.Entities;
using Mediator;

namespace Faryma.Composer.Application.Features.ComposerStream.Start
{
    public sealed record StartCommand : IRequest<ComposerStreamEntity>
    {
        /// <summary>
        /// Id стрима
        /// </summary>
        public required long ComposerStreamId { get; init; }
    }
}
