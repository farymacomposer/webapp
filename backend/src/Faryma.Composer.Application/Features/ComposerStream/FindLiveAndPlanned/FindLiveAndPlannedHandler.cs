using Faryma.Composer.Domain.Entities;
using Faryma.Composer.Infrastructure.Features.ComposerStream;
using Mediator;

namespace Faryma.Composer.Application.Features.ComposerStream.FindLiveAndPlanned
{
    public sealed class FindLiveAndPlannedHandler(ComposerStreamStore composerStreamStore) : IRequestHandler<FindLiveAndPlannedQuery, IReadOnlyCollection<ComposerStreamEntity>>
    {
        public async ValueTask<IReadOnlyCollection<ComposerStreamEntity>> Handle(FindLiveAndPlannedQuery query, CancellationToken ct) =>
            await composerStreamStore.FindLiveAndPlannedStreams(ct);
    }
}
