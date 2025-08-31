namespace Faryma.Composer.Desktop.Services.ComposerStreamFeature.Find
{
    /// <summary>
    /// Запрос поиска стримов
    /// </summary>
    public sealed record FindComposerStreamRequest
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