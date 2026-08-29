using Faryma.Composer.Domain.Entities;
using Mediator;

namespace Faryma.Composer.Application.Features.ComposerStream.Complete
{
    public sealed record CompleteCommand : IRequest<ComposerStreamEntity>
    {
        /// <summary>
        /// Id стрима
        /// </summary>
        public required long ComposerStreamId { get; init; }
    }
}
