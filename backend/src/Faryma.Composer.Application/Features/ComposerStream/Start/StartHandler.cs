using Faryma.Composer.Application.Features.OrderQueue;
using Faryma.Composer.Application.Features.OrderQueue.Enums;
using Faryma.Composer.Domain.Entities;
using Faryma.Composer.Domain.Enums;
using Faryma.Composer.Domain.Exceptions;
using Faryma.Composer.Infrastructure;
using Faryma.Composer.Infrastructure.Features.ComposerStream;
using Mediator;

namespace Faryma.Composer.Application.Features.ComposerStream.Start
{
    public sealed class StartHandler(
        ComposerStreamStore composerStreamStore,
        DateTimeContext dateTimeContext,
        AppDbContext appDbContext,
        OrderQueueEventChannel orderQueueEventChannel)
        : IRequestHandler<StartCommand, ComposerStreamEntity>
    {
        public async ValueTask<ComposerStreamEntity> Handle(StartCommand command, CancellationToken ct)
        {
            // TODO: если дата стрима не совпадает с текущей датой, то нельзя запустить

            ComposerStreamEntity stream = await composerStreamStore.GetStream(command.ComposerStreamId, ct);

            stream.ThrowIfCannotBeStart();

            ComposerStreamEntity? live = await composerStreamStore.FindLiveStream(ct);
            if (live is not null && live.Id != command.ComposerStreamId)
            {
                throw new ComposerStreamException($"Невозможно начать стрим, пока стрим на дату: {live.EventDate} запущен", stream);
            }

            if (stream.Status == ComposerStreamStatus.Live)
            {
                return stream;
            }

            stream.Start(dateTimeContext.Now);

            await appDbContext.SaveChangesAsync(ct);

            orderQueueEventChannel.Write(stream, OrderQueueUpdateType.StreamStarted);

            return stream;
        }
    }
}
