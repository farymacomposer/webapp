using Faryma.Composer.Core.Features.ComposerStreamFeature.Commands;
using Faryma.Composer.Core.Features.OrderQueueFeature;
using Faryma.Composer.Core.Utils;
using Faryma.Composer.Infrastructure;
using Faryma.Composer.Infrastructure.Entities;
using Faryma.Composer.Infrastructure.Enums;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Faryma.Composer.Core.Features.ComposerStreamFeature
{
    public sealed class ComposerStreamService(UnitOfWork uow, OrderQueueService orderQueueService)
    {
        public Task<ComposerStream[]> Find(DateOnly dateFrom, DateOnly dateTo) => uow.ComposerStreamRepository.Find(dateFrom, dateTo);
        public Task<ComposerStream[]> FindCurrentAndScheduled() => uow.ComposerStreamRepository.FindLiveAndPlanned();

        public async Task<ComposerStream> Create(CreateCommand command)
        {
            try
            {
                ComposerStream stream = uow.ComposerStreamRepository.Create(command.EventDate, command.Type);
                await uow.SaveChangesAsync();

                return stream;
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                throw new ComposerStreamException($"Стрим на дату {command.EventDate}, уже существует");
            }
        }

        public async Task<ComposerStream> Start(StartCommand command)
        {
            // TODO: если дата стрима не совпадает с текущей датой, то нельзя запустить
            ComposerStream stream = await uow.ComposerStreamRepository.Get(command.ComposerStreamId);
            if (stream.Status == ComposerStreamStatus.Live)
            {
                return stream;
            }

            if (stream.Status != ComposerStreamStatus.Planned)
            {
                throw new ComposerStreamException($"Невозможно начать стрим в статусе '{stream.Status}'");
            }

            stream.Status = ComposerStreamStatus.Live;
            stream.WentLiveAt = DateTime.UtcNow;

            await uow.SaveChangesAsync();

            ReviewOrder[] orders = await uow.ReviewOrderRepository.GetOrdersForStream(stream.Id);
            await orderQueueService.StartStream(stream, orders);

            return stream;
        }

        public async Task<ComposerStream> Complete(CompleteCommand command)
        {
            ComposerStream stream = await uow.ComposerStreamRepository.Get(command.ComposerStreamId);
            if (stream.Status == ComposerStreamStatus.Completed)
            {
                return stream;
            }

            if (stream.Status != ComposerStreamStatus.Live)
            {
                throw new ComposerStreamException($"Невозможно завершить стрим в статусе '{stream.Status}'");
            }

            stream.Status = ComposerStreamStatus.Completed;
            stream.CompletedAt = DateTime.UtcNow;

            await uow.SaveChangesAsync();

            return stream;
        }

        public async Task<ComposerStream> Cancel(CancelCommand command)
        {
            ComposerStream stream = await uow.ComposerStreamRepository.Get(command.ComposerStreamId);
            if (stream.Status == ComposerStreamStatus.Canceled)
            {
                return stream;
            }

            if (stream.Status != ComposerStreamStatus.Planned)
            {
                throw new ComposerStreamException($"Невозможно отменить стрим в статусе '{stream.Status}'");
            }

            stream.Status = ComposerStreamStatus.Canceled;

            await uow.SaveChangesAsync();

            return stream;
        }

        public async Task<ComposerStream> GetOrCreateForOrder(UserNickname userNickname, ReviewOrderType orderType)
        {
            if (orderType == ReviewOrderType.Charity)
            {
                ComposerStream? live = await uow.ComposerStreamRepository.FindLive();
                if (live is null || live.Type == ComposerStreamType.Charity)
                {
                    throw new ComposerStreamException("Благотворительный заказ можно создать только на благотворительном стриме");
                }
            }

            const DayOfWeek debtStreamDay = DayOfWeek.Tuesday;
            const DayOfWeek donationStreamDay = DayOfWeek.Saturday;

            DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
            DateOnly debtStreamDate = today.GetNextDateForDay(debtStreamDay);
            DateOnly donationStreamDate = today.GetNextDateForDay(donationStreamDay);

            (DateOnly EventDate, ComposerStreamType Type) debtStreamInfo = (debtStreamDate, ComposerStreamType.Debt);
            (DateOnly EventDate, ComposerStreamType Type) donationStreamInfo = (donationStreamDate, ComposerStreamType.Donation);
            (DateOnly EventDate, ComposerStreamType Type) nearestStreamInfo = (debtStreamDate < donationStreamDate) ? debtStreamInfo : donationStreamInfo;

            ComposerStream? nearestStream = await uow.ComposerStreamRepository.FindNearest(today);

            switch (orderType)
            {
                case ReviewOrderType.OutOfQueue:

                    return nearestStream ?? await GetOrCreateStream(nearestStreamInfo);

                case ReviewOrderType.Donation or ReviewOrderType.Free:

                    if (await uow.UserNicknameRepository.HasOrders(userNickname))
                    {
                        return await GetOrCreateStream(donationStreamInfo);
                    }
                    else
                    {
                        return nearestStream ?? await GetOrCreateStream(nearestStreamInfo);
                    }

                default:
                    throw new ComposerStreamException($"Тип заказа '{orderType}' не поддерживается");
            }
        }

        private async Task<ComposerStream> GetOrCreateStream((DateOnly EventDate, ComposerStreamType Type) streamInfo)
        {
            while (true)
            {
                DateOnly eventDate = streamInfo.EventDate;
                ComposerStream? stream = await uow.ComposerStreamRepository.Find(eventDate);

                if (stream is null)
                {
                    stream = uow.ComposerStreamRepository.Create(eventDate, streamInfo.Type);

                    try
                    {
                        await uow.SaveChangesAsync();

                        return stream;
                    }
                    catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == PostgresErrorCodes.UniqueViolation)
                    {
                        uow.Remove(stream);

                        return await uow.ComposerStreamRepository.Get(eventDate);
                    }
                }
                else if (stream.Status == ComposerStreamStatus.Planned && stream.Type == streamInfo.Type)
                {
                    return stream;
                }

                eventDate = eventDate.AddDays(6);
            }
        }
    }
}