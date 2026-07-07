using Faryma.Composer.Domain.Enums;

namespace Faryma.Composer.Application.Features.ComposerStream.Commands
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

        /// <summary>
        /// Id пользователя, создавшего стрим
        /// </summary>
        public required Guid CreatedByUserId { get; init; }
    }
}
