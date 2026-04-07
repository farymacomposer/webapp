using Faryma.Composer.Application.Features.OrderQueue;
using Faryma.Composer.Contracts.Application.Features.ComposerStream;
using Faryma.Composer.Contracts.Application.Features.ComposerStream.Commands;
using Faryma.Composer.Contracts.Application.Features.OrderQueue.Enums;
using Faryma.Composer.Contracts.Application.Features.OrderQueue.Events;
using Faryma.Composer.Contracts.Infrastructure.Entities;
using Faryma.Composer.Contracts.Infrastructure.Entities.TransactionSources;
using Faryma.Composer.Contracts.Infrastructure.Enums;
using Faryma.Composer.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Faryma.Composer.Application.Features.ComposerStream
{
    public sealed class ComposerStreamService(
        UnitOfWork uow,
        UserManager<UserEntity> userManager,
        OrderQueueEventChannel orderQueueEventChannel)
    {
        public Task<List<ComposerStreamEntity>> Find(DateOnly dateFrom, DateOnly dateTo, CancellationToken ct) => uow.ComposerStreamQueries.Find(dateFrom, dateTo, ct);
        public Task<List<ComposerStreamEntity>> FindLiveAndPlanned(CancellationToken ct) => uow.ComposerStreamQueries.FindLiveAndPlanned(ct);

        public async Task<ComposerStreamEntity> Create(CreateCommand command, CancellationToken ct)
        {
            try
            {
                UserEntity createdByUser = await userManager.Users.FirstOrDefaultAsync(x => x.Id == command.CreatedByUserId, ct)
                    ?? throw new ComposerStreamException($"Пользователь с id: {command.CreatedByUserId} не найден");

                ComposerStreamEntity stream = uow.ComposerStreamStore.Create(command.EventDate, command.Type, createdByUser);
                await uow.SaveChanges(ct);

                orderQueueEventChannel.Write(new ComposerStreamChangedEvent(stream, OrderQueueUpdateType.StreamCreated));

                return stream;
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                throw new ComposerStreamException($"Стрим на дату {command.EventDate}, уже существует");
            }
        }

        public async Task<ComposerStreamEntity> Start(long composerStreamId, DateTime now, CancellationToken ct)
        {
            // TODO: если дата стрима не совпадает с текущей датой, то нельзя запустить

            ComposerStreamEntity stream = await GetStream(composerStreamId, ct);

            if (stream.Status == ComposerStreamStatus.Live)
            {
                return stream;
            }

            if (stream.Status != ComposerStreamStatus.Planned)
            {
                throw new ComposerStreamException($"Невозможно начать стрим в статусе '{stream.Status}'", stream);
            }

            ComposerStreamEntity? live = await uow.ComposerStreamQueries.FindLive(ct);
            if (live is not null && live.Id != composerStreamId)
            {
                throw new ComposerStreamException($"Невозможно начать стрим, пока стрим на дату: {live.EventDate} запущен", stream);
            }

            stream.Status = ComposerStreamStatus.Live;
            stream.StartedAt = now;

            await uow.SaveChanges(ct);

            orderQueueEventChannel.Write(new ComposerStreamChangedEvent(stream, OrderQueueUpdateType.StreamStarted));

            return stream;
        }

        public async Task<ComposerStreamEntity> Complete(long composerStreamId, DateTime now, CancellationToken ct)
        {
            ComposerStreamEntity stream = await GetStream(composerStreamId, ct);

            if (stream.Status == ComposerStreamStatus.Completed)
            {
                return stream;
            }

            if (stream.Status != ComposerStreamStatus.Live)
            {
                throw new ComposerStreamException($"Невозможно завершить стрим в статусе '{stream.Status}'", stream);
            }

            ReviewOrderEntity? inProgress = await uow.ReviewOrderQueries.FindInProgress(ct);
            if (inProgress is not null)
            {
                throw new ComposerStreamException($"Невозможно завершить стрим, пока заказ Id: {inProgress.Id} находится в работе", stream);
            }

            stream.Status = ComposerStreamStatus.Completed;
            stream.CompletedAt = now;

            await uow.SaveChanges(ct);

            orderQueueEventChannel.Write(new ComposerStreamChangedEvent(stream, OrderQueueUpdateType.StreamCompleted));

            return stream;
        }

        public async Task<ComposerStreamEntity> Cancel(long composerStreamId, CancellationToken ct)
        {
            // TODO: отмена только если нет заказов на этот стрим

            ComposerStreamEntity stream = await GetStream(composerStreamId, ct);

            if (stream.Status == ComposerStreamStatus.Canceled)
            {
                return stream;
            }

            if (stream.Status != ComposerStreamStatus.Planned)
            {
                throw new ComposerStreamException($"Невозможно отменить стрим в статусе '{stream.Status}'", stream);
            }

            stream.Status = ComposerStreamStatus.Canceled;

            await uow.SaveChanges(ct);

            orderQueueEventChannel.Write(new ComposerStreamChangedEvent(stream, OrderQueueUpdateType.StreamCanceled));

            return stream;
        }

        private async Task<ComposerStreamEntity> GetStream(long composerStreamId, CancellationToken ct)
        {
            return await uow.ComposerStreamStore.FindById(composerStreamId, ct)
                ?? throw new ComposerStreamException($"Стрим с id: {composerStreamId} не найден");
        }
    }
}