using Faryma.Composer.Application.Features.OrderQueue;
using Faryma.Composer.Application.Features.OrderQueue.Enums;
using Faryma.Composer.Domain.Entities;
using Faryma.Composer.Domain.Entities.TransactionSources;
using Faryma.Composer.Domain.Enums;
using Faryma.Composer.Domain.Exceptions;
using Faryma.Composer.Infrastructure;
using Faryma.Composer.Infrastructure.Features.ComposerStream;
using Faryma.Composer.Infrastructure.Features.ReviewOrder;
using Mediator;

namespace Faryma.Composer.Application.Features.ComposerStream.Complete
{
    public sealed class CompleteHandler(
        ComposerStreamStore composerStreamStore,
        ReviewOrderStore reviewOrderStore,
        DateTimeContext dateTimeContext,
        AppDbContext appDbContext,
        OrderQueueEventChannel orderQueueEventChannel)
        : IRequestHandler<CompleteCommand, ComposerStreamEntity>
    {
        public async ValueTask<ComposerStreamEntity> Handle(CompleteCommand command, CancellationToken ct)
        {
            ComposerStreamEntity stream = await composerStreamStore.GetStream(command.ComposerStreamId, ct);

            if (stream.Status == ComposerStreamStatus.Completed)
            {
                return stream;
            }

            ReviewOrderEntity? orderInProgress = await reviewOrderStore.FindOrderInProgress(ct);
            if (orderInProgress is not null)
            {
                throw new ComposerStreamException($"Невозможно завершить стрим, пока заказ Id: {orderInProgress.Id} находится в работе", stream);
            }

            stream.Complete(dateTimeContext.Now);

            await appDbContext.SaveChangesAsync(ct);

            orderQueueEventChannel.Write(stream, OrderQueueUpdateType.StreamCompleted);

            return stream;
        }
    }
}
