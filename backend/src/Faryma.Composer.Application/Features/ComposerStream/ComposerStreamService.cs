using Faryma.Composer.Application.Features.OrderQueueFeature;
using Faryma.Composer.Contracts.Application.Features.ComposerStream;
using Faryma.Composer.Contracts.Application.Features.ComposerStream.Commands;
using Faryma.Composer.Contracts.Infrastructure.Entities;
using Faryma.Composer.Contracts.Infrastructure.Enums;
using Faryma.Composer.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Faryma.Composer.Application.Features.ComposerStream
{
    public sealed class ComposerStreamService(UnitOfWork uow, OrderQueueService orderQueueService)
    {
        public Task<List<ComposerStreamEntity>> Find(DateOnly dateFrom, DateOnly dateTo) => uow.ComposerStreamRead.Find(dateFrom, dateTo);
        public Task<List<ComposerStreamEntity>> FindLiveAndPlanned() => uow.ComposerStreamRead.FindLiveAndPlanned();

        public async Task<ComposerStreamEntity> Create(CreateCommand command)
        {
            try
            {
                ComposerStreamEntity stream = uow.ComposerStreamWrite.Create(command.EventDate, command.Type);
                await uow.SaveChangesAsync();

                await orderQueueService.CreateStream(stream);

                return stream;
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                throw new ComposerStreamException($"Стрим на дату {command.EventDate}, уже существует");
            }
        }

        public async Task<ComposerStreamEntity> Start(long composerStreamId)
        {
            // TODO: если дата стрима не совпадает с текущей датой, то нельзя запустить

            ComposerStreamEntity stream = await uow.ComposerStreamWrite.Get(composerStreamId);
            if (stream.Status == ComposerStreamStatus.Live)
            {
                return stream;
            }

            if (stream.Status != ComposerStreamStatus.Planned)
            {
                throw new ComposerStreamException($"Невозможно начать стрим в статусе '{stream.Status}'", stream);
            }

            ComposerStreamEntity? live = await uow.ComposerStreamRead.FindLive();
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

        public async Task<ComposerStreamEntity> Complete(long composerStreamId)
        {
            ComposerStreamEntity stream = await uow.ComposerStreamWrite.Get(composerStreamId);
            if (stream.Status == ComposerStreamStatus.Completed)
            {
                return stream;
            }

            if (stream.Status != ComposerStreamStatus.Live)
            {
                throw new ComposerStreamException($"Невозможно завершить стрим в статусе '{stream.Status}'", stream);
            }

            ReviewOrderEntity? inProgress = await uow.ReviewOrderRead.FindInProgress();
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

        public async Task<ComposerStreamEntity> Cancel(long composerStreamId)
        {
            // TODO: отмена только если нет заказов на этот стрим

            ComposerStreamEntity stream = await uow.ComposerStreamWrite.Get(composerStreamId);
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

            await orderQueueService.CancelStream();

            return stream;
        }
    }
}