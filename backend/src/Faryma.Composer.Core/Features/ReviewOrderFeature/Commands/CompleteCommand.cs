namespace Faryma.Composer.Core.Features.ReviewOrderFeature.Commands
{
    /// <summary>
    /// Команда выполнения заказа
    /// </summary>
    public sealed record CompleteCommand
    {
        /// <summary>
        /// Id заказа разбора трека
        /// </summary>
        public required long ReviewOrderId { get; init; }

        /// <summary>
        /// Оценка трека (0-26)
        /// </summary>
        public required int Rating { get; init; }

        /// <summary>
        /// Комментарий к разбору
        /// </summary>
        public required string Comment { get; init; }
    }
}