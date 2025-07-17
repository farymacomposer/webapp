using Faryma.Composer.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Faryma.Composer.Infrastructure.Repositories
{
    public sealed class ArtistRepository(AppDbContext context, ILookupNormalizer normalizer)
    {
        public Artist Create(string name)
        {
            return context.Add(new Artist
            {
                Name = name,
                NormalizedName = normalizer.NormalizeName(name),
            }).Entity;
        }

        public Task<Artist?> Find(string name)
        {
            string normalized = normalizer.NormalizeName(name);

            return context.Artists.FirstOrDefaultAsync(x => x.NormalizedName == normalized);
        }
    }
}