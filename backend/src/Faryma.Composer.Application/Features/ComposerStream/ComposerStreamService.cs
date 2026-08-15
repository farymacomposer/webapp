using Faryma.Composer.Application.Features.ComposerStream.Commands;
using Faryma.Composer.Application.Features.OrderQueue;
using Faryma.Composer.Application.Features.ReviewOrder;
using Faryma.Composer.Application.SharedContracts.Features.OrderQueue.Enums;
using Faryma.Composer.Domain.Entities;
using Faryma.Composer.Domain.Enums;
using Faryma.Composer.Domain.Exceptions;
using Faryma.Composer.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Faryma.Composer.Application.Features.ComposerStream
{
    public sealed class ComposerStreamService(
        UnitOfWork uow,
        ReviewOrderService reviewOrderService,
        UserManager<UserEntity> userManager,
        OrderQueueEventChannel orderQueueEventChannel,
        DateTimeService dateTimeService)
    {
        /// <summary>
        /// Возвращает стримы в указанном диапазоне дат
        /// </summary>
        public Task<List<ComposerStreamEntity>> Find(DateOnly dateFrom, DateOnly dateTo, CancellationToken ct)
        {
            return uow.Context.ComposerStreams
                .AsNoTracking()
                .Where(x => x.EventDate >= dateFrom && x.EventDate <= dateTo)
                .OrderBy(x => x.EventDate)
                .ToListAsync(ct);
        }

        /// <summary>
        /// Возвращает список актуальных стримов: Live и Planned на сегодня/будущее
        /// </summary>
        public Task<List<ComposerStreamEntity>> FindLiveAndPlanned(CancellationToken ct)
        {
            DateOnly today = dateTimeService.Today;

            IQueryable<ComposerStreamEntity> query = uow.Context.ComposerStreams
                .AsNoTracking()
                .Where(x => x.Status == ComposerStreamStatus.Live
                    || (x.Status == ComposerStreamStatus.Planned && x.EventDate >= today))
                .OrderBy(x => x.EventDate);

            return query.ToListAsync(ct);
        }

        public async Task<ComposerStreamEntity> Create(CreateCommand command, CancellationToken ct = default)
        {
            try
            {
                UserEntity createdByUser = await userManager.Users.FirstOrDefaultAsync(x => x.Id == command.CreatedByUserId, ct)
                    ?? throw new ComposerStreamException($"Пользователь с id: {command.CreatedByUserId} не найден");

                ComposerStreamEntity stream = uow.ComposerStreamStore.Create(command.EventDate, command.Type, createdByUser);
                await uow.SaveChanges(ct);

                orderQueueEventChannel.Write(stream, OrderQueueUpdateType.StreamCreated);

                return stream;
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                throw new ComposerStreamException($"Стрим на дату {command.EventDate}, уже существует");
            }
        }

        public async Task<ComposerStreamEntity> Start(long composerStreamId, CancellationToken ct = default)
        {
            // TODO: если дата стрима не совпадает с текущей датой, то нельзя запустить

            ComposerStreamEntity stream = await uow.ComposerStreamStore.Get(composerStreamId, ct);

            if (stream.Status == ComposerStreamStatus.Live)
            {
                return stream;
            }

            if (stream.Status != ComposerStreamStatus.Planned)
            {
                throw new ComposerStreamException($"Невозможно начать стрим в статусе '{stream.Status}'", stream);
            }

            ComposerStreamEntity? live = await FindLive(ct);
            if (live is not null && live.Id != composerStreamId)
            {
                throw new ComposerStreamException($"Невозможно начать стрим, пока стрим на дату: {live.EventDate} запущен", stream);
            }

            stream.Status = ComposerStreamStatus.Live;
            stream.StartedAt = dateTimeService.Now;

            await uow.SaveChanges(ct);

            orderQueueEventChannel.Write(stream, OrderQueueUpdateType.StreamStarted);

            return stream;
        }

        public async Task<ComposerStreamEntity> Complete(long composerStreamId, CancellationToken ct = default)
        {
            ComposerStreamEntity stream = await uow.ComposerStreamStore.Get(composerStreamId, ct);

            if (stream.Status == ComposerStreamStatus.Completed)
            {
                return stream;
            }

            if (stream.Status != ComposerStreamStatus.Live)
            {
                throw new ComposerStreamException($"Невозможно завершить стрим в статусе '{stream.Status}'", stream);
            }

            long? idOrderInProgress = await reviewOrderService.FindInProgress(ct);
            if (idOrderInProgress is not null)
            {
                throw new ComposerStreamException($"Невозможно завершить стрим, пока заказ Id: {idOrderInProgress} находится в работе", stream);
            }

            stream.Status = ComposerStreamStatus.Completed;
            stream.CompletedAt = dateTimeService.Now;

            await uow.SaveChanges(ct);

            orderQueueEventChannel.Write(stream, OrderQueueUpdateType.StreamCompleted);

            return stream;
        }

        public async Task<ComposerStreamEntity> Cancel(long composerStreamId, CancellationToken ct = default)
        {
            ComposerStreamEntity stream = await uow.ComposerStreamStore.Get(composerStreamId, ct);

            if (stream.Status == ComposerStreamStatus.Canceled)
            {
                return stream;
            }

            if (stream.Status != ComposerStreamStatus.Planned)
            {
                throw new ComposerStreamException($"Невозможно отменить стрим в статусе '{stream.Status}'", stream);
            }

            bool hasActiveCreatedOrders = await ExistsActiveCreatedOrdersForStream(stream.Id, ct);
            if (hasActiveCreatedOrders)
            {
                throw new ComposerStreamException("Невозможно отменить стрим: для него существуют активные заказы", stream);
            }

            stream.Status = ComposerStreamStatus.Canceled;

            await uow.SaveChanges(ct);

            orderQueueEventChannel.Write(stream, OrderQueueUpdateType.StreamCanceled);

            return stream;
        }

        /// <summary>
        /// Проверяет, есть ли у стрима активные созданные заказы
        /// </summary>
        private Task<bool> ExistsActiveCreatedOrdersForStream(long streamId, CancellationToken ct = default)
        {
            return uow.Context.ReviewOrders
                .AnyAsync(x => x.CreationStreamId == streamId
                    && (x.Status == ReviewOrderStatus.Preorder
                        || x.Status == ReviewOrderStatus.Pending
                        || x.Status == ReviewOrderStatus.AwaitingPayment), ct);
        }

        /// <summary>
        /// Возвращает текущий стрим в статусе Live, если он существует
        /// </summary>
        private Task<ComposerStreamEntity?> FindLive(CancellationToken ct)
        {
            return uow.Context.ComposerStreams
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Status == ComposerStreamStatus.Live, ct);
        }
    }
}
