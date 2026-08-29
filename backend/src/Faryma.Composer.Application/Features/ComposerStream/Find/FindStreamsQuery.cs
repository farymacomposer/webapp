using Faryma.Composer.Domain.Entities;
using Mediator;

namespace Faryma.Composer.Application.Features.ComposerStream.Find
{
    public sealed record FindStreamsQuery : IRequest<IReadOnlyCollection<ComposerStreamEntity>>
    {
        /// <summary>
        /// Начальная дата периода поиска
        /// </summary>
        public required DateOnly DateFrom { get; init; }

        /// <summary>
        /// Конечная дата периода поиска
        /// </summary>
        public required DateOnly DateTo { get; init; }
    }
}
