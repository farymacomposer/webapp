using Faryma.Composer.Infrastructure.Enums;

namespace Faryma.Composer.Desktop.Services.ComposerStreamFeature.Create
{
    /// <summary>
    /// Запрос создания стрима
    /// </summary>
    public sealed record CreateComposerStreamRequest
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