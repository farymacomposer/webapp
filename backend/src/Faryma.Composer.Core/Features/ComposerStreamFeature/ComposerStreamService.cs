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
        public const DayOfWeek DebtStreamDay = DayOfWeek.Tuesday;
        public const DayOfWeek DonationStreamDay = DayOfWeek.Saturday;

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
            DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
            DateOnly debtStreamDate = today.GetNextDateForDay(DebtStreamDay);
            DateOnly donationStreamDate = today.GetNextDateForDay(DonationStreamDay);

            (DateOnly EventDate, ComposerStreamType Type) nearestStreamInfo = (debtStreamDate < donationStreamDate)
                ? (debtStreamDate, ComposerStreamType.Debt)
                : (donationStreamDate, ComposerStreamType.Donation);

            ComposerStream? nearestStream = await ofw.ComposerStreamRepository.FindNearestInWeekRange(today);

            switch (orderType)
            {
                case ReviewOrderType.OutOfQueue:
                    return nearestStream ?? await GetOrCreateStream(nearestStreamInfo.EventDate, nearestStreamInfo.Type);

                case ReviewOrderType.Donation or ReviewOrderType.Free:
                    if (await ofw.UserNicknameRepository.HasOrders(userNickname))
                    {
                        return await GetOrCreateStream(donationStreamDate, ComposerStreamType.Donation);
                    }

                    return nearestStream ?? await GetOrCreateStream(nearestStreamInfo.EventDate, nearestStreamInfo.Type);

                default:
                    throw new ComposerStreamException($"Типа заказа {orderType} не поддерживается");
            }
        }

        private async Task<ComposerStream> GetOrCreateStream(DateOnly eventDate, ComposerStreamType streamType)
        {
            ComposerStream? stream = await ofw.ComposerStreamRepository.Find(eventDate);
            if (stream is not null)
            {
                return stream;
            }

            stream = ofw.ComposerStreamRepository.Create(eventDate, streamType);

            try
            {
                await ofw.SaveChangesAsync();

                return stream;
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                ofw.Remove(stream);

                return await ofw.ComposerStreamRepository.Get(eventDate);
            }
        }
    }
}