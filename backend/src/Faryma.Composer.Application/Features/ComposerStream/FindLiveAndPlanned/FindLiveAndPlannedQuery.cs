using Faryma.Composer.Domain.Entities;
using Mediator;

namespace Faryma.Composer.Application.Features.ComposerStream.FindLiveAndPlanned
{
    public sealed record FindLiveAndPlannedQuery : IRequest<IReadOnlyCollection<ComposerStreamEntity>>
    {
    }
}
