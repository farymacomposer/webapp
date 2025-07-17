using Faryma.Composer.Infrastructure;
using Faryma.Composer.Infrastructure.Entities;

namespace Faryma.Composer.Core.Features.GenreFeature
{
    public sealed class GenreService(UnitOfWork ofw)
    {
        public async Task<Genre> GetOrCreate(string name)
        {
            Genre? result = await ofw.GenreRepository.Find(name);

            if (result is null)
            {
                result = ofw.GenreRepository.Create(name);
                await ofw.SaveChangesAsync();
            }

            return result;
        }
    }
}