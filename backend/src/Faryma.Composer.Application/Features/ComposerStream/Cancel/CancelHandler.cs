using Faryma.Composer.Application.Features.OrderQueue;
using Faryma.Composer.Application.SharedContracts.Features.OrderQueue.Enums;
using Faryma.Composer.Domain.Entities;
using Faryma.Composer.Domain.Enums;
using Faryma.Composer.Domain.Exceptions;
using Faryma.Composer.Infrastructure;
using Faryma.Composer.Infrastructure.Features.ComposerStream;
using Mediator;

namespace Faryma.Composer.Application.Features.ComposerStream.Cancel
{
    public sealed class CancelHandler(
        AppDbContext context,
        ComposerStreamStore composerStreamStore,
        OrderQueueEventChannel orderQueueEventChannel) : IRequestHandler<CancelCommand, ComposerStreamEntity>
    {
        public async ValueTask<ComposerStreamEntity> Handle(CancelCommand command, CancellationToken ct)
        {
            ComposerStreamEntity stream = await composerStreamStore.GetStream(command.ComposerStreamId, ct);

            if (stream.Status == ComposerStreamStatus.Canceled)
            {
                return stream;
            }

            bool hasActiveCreatedOrders = await composerStreamStore.ExistsActiveCreatedOrdersForStream(stream.Id, ct);
            if (hasActiveCreatedOrders)
            {
                throw new ComposerStreamException("Невозможно отменить стрим: для него существуют активные заказы", stream);
            }

            stream.Cancel();

            await context.SaveChangesAsync(ct);

            orderQueueEventChannel.Write(stream, OrderQueueUpdateType.StreamCanceled);

            return stream;
        }
    }
}
