using Faryma.Composer.Domain.Entities;
using Mediator;

namespace Faryma.Composer.Application.Features.ComposerStream.FindLiveAndPlanned
{
    public sealed record FindLiveAndPlannedStreamsQuery : IRequest<IReadOnlyCollection<ComposerStreamEntity>>
    {
    }
}
