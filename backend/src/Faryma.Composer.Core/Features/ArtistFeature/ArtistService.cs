using Faryma.Composer.Infrastructure;
using Faryma.Composer.Infrastructure.Entities;

namespace Faryma.Composer.Core.Features.ArtistFeature
{
    public sealed class ArtistService(UnitOfWork ofw)
    {
        public async Task<Artist> GetOrCreate(string name)
        {
            Artist? result = await ofw.ArtistRepository.Find(name);

            if (result is null)
            {
                result = ofw.ArtistRepository.Create(name);
                await ofw.SaveChangesAsync();
            }

            return result;
        }
    }
}