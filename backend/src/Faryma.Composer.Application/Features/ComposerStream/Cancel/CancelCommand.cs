using Faryma.Composer.Domain.Entities;
using Mediator;

namespace Faryma.Composer.Application.Features.ComposerStream.Cancel
{
    /// <summary>
    /// Команда отмены стрима
    /// </summary>
    public sealed record CancelCommand : IRequest<ComposerStreamEntity>
    {
        /// <summary>
        /// Id стрима
        /// </summary>
        public required long ComposerStreamId { get; init; }
    }
}
