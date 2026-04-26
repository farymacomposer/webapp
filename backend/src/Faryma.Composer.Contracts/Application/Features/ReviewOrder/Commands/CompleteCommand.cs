namespace Faryma.Composer.Contracts.Application.Features.ReviewOrder.Commands
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
        /// Id пользователя, создавшего разбор
        /// </summary>
        public required Guid CreatedByUserId { get; init; }
    }
}
