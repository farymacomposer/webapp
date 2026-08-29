using Faryma.Composer.Domain.Entities;
using Faryma.Composer.Infrastructure.Features.ComposerStream;
using Mediator;

namespace Faryma.Composer.Application.Features.ComposerStream.FindLiveAndPlanned
{
    public sealed class FindLiveAndPlannedStreamsHandler(ComposerStreamStore composerStreamStore) : IRequestHandler<FindLiveAndPlannedStreamsQuery, IReadOnlyCollection<ComposerStreamEntity>>
    {
        public async ValueTask<IReadOnlyCollection<ComposerStreamEntity>> Handle(FindLiveAndPlannedStreamsQuery query, CancellationToken ct) =>
            await composerStreamStore.FindLiveAndPlannedStreams(ct);
    }
}
