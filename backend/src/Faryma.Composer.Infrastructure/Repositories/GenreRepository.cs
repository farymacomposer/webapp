using Faryma.Composer.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Faryma.Composer.Infrastructure.Repositories
{
    public sealed class GenreRepository(AppDbContext context, ILookupNormalizer normalizer)
    {
        public Genre Create(string name)
        {
            return context.Add(new Genre
            {
                Name = name,
                NormalizedName = normalizer.NormalizeName(name),
            }).Entity;
        }

        public Task<Genre?> Find(string name)
        {
            string normalized = normalizer.NormalizeName(name);

            return context.Genres.FirstOrDefaultAsync(x => x.NormalizedName == normalized);
        }
    }
}