using Faryma.Composer.Infrastructure;
using Faryma.Composer.Infrastructure.Entities;
using Faryma.Composer.Infrastructure.Enums;

namespace Faryma.Composer.Core.Features.ComposerStreamFeature
{
    public sealed class ComposerStreamService(UnitOfWork ofw)
    {
        public async Task<ComposerStream> Create(DateOnly eventDate, ComposerStreamType type)
        {
            if (await ofw.ComposerStreamRepository.Exists(eventDate))
            {
                throw new ComposerStreamException($"Стрим на дату {eventDate}, уже существует");
            }

            ComposerStream result = ofw.ComposerStreamRepository.Create(eventDate, type);
            await ofw.SaveChangesAsync();

            return result;
        }

        public Task<IReadOnlyCollection<ComposerStream>> Find(DateOnly dateFrom, DateOnly dateTo) => ofw.ComposerStreamRepository.Find(dateFrom, dateTo);

        public ComposerStream GetStreamForOrder() => null!;
    }
}