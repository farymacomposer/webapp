using Faryma.Composer.Domain.Entities.TransactionSources;
using Mediator;

namespace Faryma.Composer.Application.Features.ReviewOrder.AddTrackUrl
{
    /// <summary>
    /// Команда добавления/изменения ссылки на трек в заказе
    /// </summary>
    public sealed record AddTrackUrlCommand : IRequest<ReviewOrderEntity>
    {
        /// <summary>
        /// Id заказа разбора трека
        /// </summary>
        public required long ReviewOrderId { get; init; }

        /// <summary>
        /// Ссылка на трек
        /// </summary>
        public required string TrackUrl { get; init; }

        /// <summary>
        /// Длительность трека в секундах
        /// </summary>
        public required int TrackDurationSeconds { get; init; }
    }
}
