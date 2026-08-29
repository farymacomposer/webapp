using Faryma.Composer.Application.Features.OrderQueue;
using Faryma.Composer.Application.Features.ReviewOrder;
using Faryma.Composer.Application.SharedContracts.Features.OrderQueue.Enums;
using Faryma.Composer.Domain.Entities;
using Faryma.Composer.Domain.Enums;
using Faryma.Composer.Domain.Exceptions;
using Faryma.Composer.Infrastructure;
using Mediator;

namespace Faryma.Composer.Application.Features.ComposerStream.Complete
{
    public sealed class CompleteHandler(
        UnitOfWork uow,
        ReviewOrderService reviewOrderService,
        DateTimeService dateTimeService,
        OrderQueueEventChannel orderQueueEventChannel) : IRequestHandler<CompleteCommand, ComposerStreamEntity>
    {
        public async ValueTask<ComposerStreamEntity> Handle(CompleteCommand command, CancellationToken ct = default)
        {
            ComposerStreamEntity stream = await uow.ComposerStreamStore.Get(command.ComposerStreamId, ct);

            if (stream.Status == ComposerStreamStatus.Completed)
            {
                return stream;
            }

            long? idOrderInProgress = await reviewOrderService.FindInProgress(ct);
            if (idOrderInProgress is not null)
            {
                throw new ComposerStreamException($"Невозможно завершить стрим, пока заказ Id: {idOrderInProgress} находится в работе", stream);
            }

            stream.Complete(dateTimeService.Now);

            await uow.SaveChanges(ct);

            orderQueueEventChannel.Write(stream, OrderQueueUpdateType.StreamCompleted);

            return stream;
        }
    }
}
