using Faryma.Composer.Domain.Entities;
using Faryma.Composer.Infrastructure.Features.ComposerStream;
using Mediator;

namespace Faryma.Composer.Application.Features.ComposerStream.Find
{
    public sealed class FindHandler(ComposerStreamQueries composerStreamQueries) : IRequestHandler<FindQuery, IReadOnlyCollection<ComposerStreamEntity>>
    {
        public async ValueTask<IReadOnlyCollection<ComposerStreamEntity>> Handle(FindQuery query, CancellationToken ct = default) =>
            await composerStreamQueries.Find(query.DateFrom, query.DateTo, ct);
    }
}
