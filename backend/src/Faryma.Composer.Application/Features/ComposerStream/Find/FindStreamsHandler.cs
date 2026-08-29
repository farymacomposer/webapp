using Faryma.Composer.Domain.Entities;
using Faryma.Composer.Infrastructure.Features.ComposerStream;
using Mediator;

namespace Faryma.Composer.Application.Features.ComposerStream.Find
{
    public sealed class FindStreamsHandler(ComposerStreamStore composerStreamStore)
        : IRequestHandler<FindStreamsQuery, IReadOnlyCollection<ComposerStreamEntity>>
    {
        public async ValueTask<IReadOnlyCollection<ComposerStreamEntity>> Handle(FindStreamsQuery query, CancellationToken ct) =>
            await composerStreamStore.FindStreams(query.DateFrom, query.DateTo, ct);
    }
}
