using Faryma.Composer.Application.Features.OrderQueue;
using Faryma.Composer.Application.SharedContracts.Features.OrderQueue.Enums;
using Faryma.Composer.Domain.Entities;
using Faryma.Composer.Domain.Enums;
using Faryma.Composer.Domain.Exceptions;
using Faryma.Composer.Infrastructure;
using Faryma.Composer.Infrastructure.Features.ComposerStream;
using Mediator;

namespace Faryma.Composer.Application.Features.ComposerStream.Start
{
    public sealed class StartHandler(
        AppDbContext context,
        DateTimeService dateTimeService,
        ComposerStreamStore composerStreamStore,
        OrderQueueEventChannel orderQueueEventChannel) : IRequestHandler<StartCommand, ComposerStreamEntity>
    {
        public async ValueTask<ComposerStreamEntity> Handle(StartCommand command, CancellationToken ct)
        {
            // TODO: если дата стрима не совпадает с текущей датой, то нельзя запустить

            ComposerStreamEntity stream = await composerStreamStore.GetStream(command.ComposerStreamId, ct);

            if (stream.Status == ComposerStreamStatus.Live)
            {
                return stream;
            }

            ComposerStreamEntity? live = await composerStreamStore.FindLiveStream(ct);
            if (live is not null && live.Id != command.ComposerStreamId)
            {
                throw new ComposerStreamException($"Невозможно начать стрим, пока стрим на дату: {live.EventDate} запущен", stream);
            }

            stream.Start(dateTimeService.Now);

            await context.SaveChangesAsync(ct);

            orderQueueEventChannel.Write(stream, OrderQueueUpdateType.StreamStarted);

            return stream;
        }
    }
}
