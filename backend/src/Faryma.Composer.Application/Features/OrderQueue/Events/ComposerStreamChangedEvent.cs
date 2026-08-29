using Faryma.Composer.Application.Features.OrderQueue.Enums;
using Faryma.Composer.Domain.Entities;

namespace Faryma.Composer.Application.Features.OrderQueue.Events
{
    /// <summary>
    /// Стрим композитора был изменен
    /// </summary>
    public sealed class ComposerStreamChangedEvent(ComposerStreamEntity stream, OrderQueueUpdateType updateType) : OrderQueueEvent(updateType)
    {
        /// <summary>
        /// Стрим композитора
        /// </summary>
        public ComposerStreamEntity Stream { get; } = stream;
    }
}
