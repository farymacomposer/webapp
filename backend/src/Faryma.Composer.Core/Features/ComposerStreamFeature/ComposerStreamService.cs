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
            DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
            (DateOnly EventDate, ComposerStreamType Type) debt = (today.GetNextDateForDay(DayOfWeek.Tuesday), ComposerStreamType.Debt);
            (DateOnly EventDate, ComposerStreamType Type) donation = (today.GetNextDateForDay(DayOfWeek.Saturday), ComposerStreamType.Donation);
            (DateOnly EventDate, ComposerStreamType Type) nearest = (debt.EventDate < donation.EventDate) ? debt : donation;

            ComposerStream? result = await ofw.ComposerStreamRepository.FindNearest(today);

            switch (orderType)
            {
                case ReviewOrderType.OutOfQueue:

                    return result ?? await GetOrCreate(nearest);

                case ReviewOrderType.Donation or ReviewOrderType.Free:

                    if (await ofw.UserNicknameRepository.HasOrders(userNickname))
                    {
                        return await GetOrCreate(donation);
                    }
                    else
                    {
                        return result ?? await GetOrCreate(nearest);
                    }

                default:
                    throw new ComposerStreamException($"Типа заказа {orderType} не поддерживается");
            }

            async Task<ComposerStream> GetOrCreate((DateOnly EventDate, ComposerStreamType Type) item)
            {
                ComposerStream result = ofw.ComposerStreamRepository.Create(item.EventDate, item.Type);

                try
                {
                    await ofw.SaveChangesAsync();

                    return result;
                }
                catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == PostgresErrorCodes.UniqueViolation)
                {
                    ofw.Remove(result);

                    return await ofw.ComposerStreamRepository.Get(item.EventDate);
                }
            }
        }
    }
}