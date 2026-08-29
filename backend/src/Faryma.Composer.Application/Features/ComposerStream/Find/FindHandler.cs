using Faryma.Composer.Domain.Entities;
using Faryma.Composer.Infrastructure.Features.ComposerStream;
using Mediator;

namespace Faryma.Composer.Application.Features.ComposerStream.Find
{
    public sealed class FindHandler(ComposerStreamStore composerStreamStore) : IRequestHandler<FindQuery, IReadOnlyCollection<ComposerStreamEntity>>
    {
        public async ValueTask<IReadOnlyCollection<ComposerStreamEntity>> Handle(FindQuery query, CancellationToken ct) =>
            await composerStreamStore.FindStreams(query.DateFrom, query.DateTo, ct);
    }
}
