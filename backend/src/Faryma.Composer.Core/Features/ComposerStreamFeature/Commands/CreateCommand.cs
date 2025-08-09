using Faryma.Composer.Infrastructure.Enums;

namespace Faryma.Composer.Core.Features.ComposerStreamFeature.Commands
{
    /// <summary>
    /// Команда создания стрима
    /// </summary>
    public sealed record CreateCommand
    {
        /// <summary>
        /// Дата проведения стрима
        /// </summary>
        public required DateOnly EventDate { get; init; }

        /// <summary>
        /// Тип стрима
        /// </summary>
        public required ComposerStreamType Type { get; init; }
    }
}