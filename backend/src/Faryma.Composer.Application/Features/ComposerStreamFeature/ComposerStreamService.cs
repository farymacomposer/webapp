using Faryma.Composer.Application.Features.ComposerStreamFeature.Commands;
using Faryma.Composer.Application.Features.OrderQueueFeature;
using Faryma.Composer.Infrastructure;
using Faryma.Composer.Infrastructure.Entities;
using Faryma.Composer.Infrastructure.Enums;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Faryma.Composer.Application.Features.ComposerStreamFeature
{
    public sealed class ComposerStreamService(UnitOfWork uow, OrderQueueService orderQueueService)
    {
        public Task<ComposerStream[]> Find(DateOnly dateFrom, DateOnly dateTo) => uow.ComposerStream_R.Find(dateFrom, dateTo);
        public Task<ComposerStream[]> FindLiveAndPlanned() => uow.ComposerStream_R.FindLiveAndPlanned();

        public async Task<ComposerStream> Create(CreateCommand command)
        {
            try
            {
                ComposerStream stream = uow.ComposerStream_RW.Create(command.EventDate, command.Type);
                await uow.SaveChangesAsync();

                return stream;
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                throw new ComposerStreamException($"Стрим на дату {command.EventDate}, уже существует");
            }
        }

        public async Task<ComposerStream> Start(long composerStreamId)
        {
            // TODO: если дата стрима не совпадает с текущей датой, то нельзя запустить
            ComposerStream stream = await uow.ComposerStream_RW.Get(composerStreamId);
            if (stream.Status == ComposerStreamStatus.Live)
            {
                return stream;
            }

            if (stream.Status != ComposerStreamStatus.Planned)
            {
                throw new ComposerStreamException($"Невозможно начать стрим в статусе '{stream.Status}'", stream);
            }

            ComposerStream? live = await uow.ComposerStream_R.FindLive();
            if (live is not null && live.Id != composerStreamId)
            {
                throw new ComposerStreamException($"Невозможно начать стрим, пока стрим на дату: {live.EventDate} запущен", stream);
            }

            stream.Status = ComposerStreamStatus.Live;
            stream.StartedAt = DateTime.UtcNow;

            await uow.SaveChangesAsync();

            await orderQueueService.StartStream(stream);

            return stream;
        }

        public async Task<ComposerStream> Complete(long composerStreamId)
        {
            ComposerStream stream = await uow.ComposerStream_RW.Get(composerStreamId);
            if (stream.Status == ComposerStreamStatus.Completed)
            {
                return stream;
            }

            if (stream.Status != ComposerStreamStatus.Live)
            {
                throw new ComposerStreamException($"Невозможно завершить стрим в статусе '{stream.Status}'", stream);
            }

            ReviewOrder? inProgress = await uow.ReviewOrder_R.FindInProgress();
            if (inProgress is not null)
            {
                throw new ComposerStreamException($"Невозможно завершить стрим, пока заказ Id: {inProgress.Id} находится в работе", stream);
            }

            stream.Status = ComposerStreamStatus.Completed;
            stream.CompletedAt = DateTime.UtcNow;

            await uow.SaveChangesAsync();

            await orderQueueService.CompleteStream(stream);

            return stream;
        }

        public async Task<ComposerStream> Cancel(long composerStreamId)
        {
            ComposerStream stream = await uow.ComposerStream_RW.Get(composerStreamId);
            if (stream.Status == ComposerStreamStatus.Canceled)
            {
                return stream;
            }

            if (stream.Status != ComposerStreamStatus.Planned)
            {
                throw new ComposerStreamException($"Невозможно отменить стрим в статусе '{stream.Status}'", stream);
            }

            stream.Status = ComposerStreamStatus.Canceled;

            await uow.SaveChangesAsync();

            return stream;
        }
    }
}