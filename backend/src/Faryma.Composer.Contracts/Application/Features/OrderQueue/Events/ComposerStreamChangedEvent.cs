using Faryma.Composer.Contracts.Application.Features.OrderQueue.Enums;
using Faryma.Composer.Contracts.Infrastructure.Entities;

namespace Faryma.Composer.Contracts.Application.Features.OrderQueue.Events
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