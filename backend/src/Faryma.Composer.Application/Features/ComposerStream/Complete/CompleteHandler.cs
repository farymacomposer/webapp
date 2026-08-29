using Faryma.Composer.Application.Features.OrderQueue;
using Faryma.Composer.Application.SharedContracts.Features.OrderQueue.Enums;
using Faryma.Composer.Domain.Entities;
using Faryma.Composer.Domain.Enums;
using Faryma.Composer.Domain.Exceptions;
using Faryma.Composer.Infrastructure;
using Faryma.Composer.Infrastructure.Features.ComposerStream;
using Mediator;

namespace Faryma.Composer.Application.Features.ComposerStream.Complete
{
    public sealed class CompleteHandler(
        AppDbContext context,
        DateTimeService dateTimeService,
        ComposerStreamStore composerStreamStore,
        OrderQueueEventChannel orderQueueEventChannel) : IRequestHandler<CompleteCommand, ComposerStreamEntity>
    {
        public async ValueTask<ComposerStreamEntity> Handle(CompleteCommand command, CancellationToken ct)
        {
            ComposerStreamEntity stream = await composerStreamStore.GetStream(command.ComposerStreamId, ct);

            if (stream.Status == ComposerStreamStatus.Completed)
            {
                return stream;
            }

            long? idOrderInProgress = await composerStreamStore.FindIdOrderInProgress(ct);
            if (idOrderInProgress is not null)
            {
                throw new ComposerStreamException($"Невозможно завершить стрим, пока заказ Id: {idOrderInProgress} находится в работе", stream);
            }

            stream.Complete(dateTimeService.Now);

            await context.SaveChangesAsync(ct);

            orderQueueEventChannel.Write(stream, OrderQueueUpdateType.StreamCompleted);

            return stream;
        }
    }
}
