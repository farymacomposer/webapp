using Faryma.Composer.Core.Utils;
using Faryma.Composer.Infrastructure;
using Faryma.Composer.Infrastructure.Entities;
using Faryma.Composer.Infrastructure.Enums;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Faryma.Composer.Core.Features.ComposerStreamFeature
{
    public sealed class ComposerStreamService(UnitOfWork ofw)
    {
        public Task<IReadOnlyCollection<ComposerStream>> Find(DateOnly dateFrom, DateOnly dateTo) => ofw.ComposerStreamRepository.Find(dateFrom, dateTo);

        public async Task<ComposerStream> Create(DateOnly eventDate, ComposerStreamType type)
        {
            try
            {
                ComposerStream result = ofw.ComposerStreamRepository.Create(eventDate, type);
                await ofw.SaveChangesAsync();

                return result;
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                throw new ComposerStreamException($"Стрим на дату {eventDate}, уже существует");
            }
        }

        public async Task<ComposerStream> GetOrCreateForOrder(UserNickname userNickname, ReviewOrderType orderType)
        {
            const DayOfWeek debtStreamDay = DayOfWeek.Tuesday;
            const DayOfWeek donationStreamDay = DayOfWeek.Saturday;

            DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
            DateOnly debtStreamDate = today.GetNextDateForDay(debtStreamDay);
            DateOnly donationStreamDate = today.GetNextDateForDay(donationStreamDay);

            (DateOnly EventDate, ComposerStreamType Type) debtStreamInfo = (debtStreamDate, ComposerStreamType.Debt);
            (DateOnly EventDate, ComposerStreamType Type) donationStreamInfo = (donationStreamDate, ComposerStreamType.Donation);
            (DateOnly EventDate, ComposerStreamType Type) nearestStreamInfo = (debtStreamDate < donationStreamDate) ? debtStreamInfo : donationStreamInfo;

            ComposerStream? nearestStream = await ofw.ComposerStreamRepository.FindNearestInWeekRange(today);

            switch (orderType)
            {
                case ReviewOrderType.OutOfQueue:

                    return nearestStream ?? await GetOrCreateStream(nearestStreamInfo);

                case ReviewOrderType.Donation or ReviewOrderType.Free:

                    if (await ofw.UserNicknameRepository.HasOrders(userNickname))
                    {
                        return await GetOrCreateStream(donationStreamInfo);
                    }
                    else
                    {
                        return nearestStream ?? await GetOrCreateStream(nearestStreamInfo);
                    }

                default:
                    throw new ComposerStreamException($"Типа заказа {orderType} не поддерживается");
            }
        }

        private async Task<ComposerStream> GetOrCreateStream((DateOnly EventDate, ComposerStreamType Type) streamInfo)
        {
            ComposerStream? stream = await ofw.ComposerStreamRepository.Find(streamInfo.EventDate);
            if (stream is not null)
            {
                return stream;
            }

            stream = ofw.ComposerStreamRepository.Create(streamInfo.EventDate, streamInfo.Type);

            try
            {
                await ofw.SaveChangesAsync();

                return stream;
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                ofw.Remove(stream);

                return await ofw.ComposerStreamRepository.Get(streamInfo.EventDate);
            }
        }
    }
}