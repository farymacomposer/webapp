using Faryma.Composer.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Faryma.Composer.Infrastructure.Repositories
{
    public sealed class TrackRepository(AppDbContext context)
    {
        public Track Create(UserNickname userNickname, string url)
        {
            return context.Add(new Track
            {
                UploadedAt = DateTime.UtcNow,
                UploadedBy = userNickname,
                Url = url,
            }).Entity;
        }

        public IQueryable<Track> GetAll() => context.Tracks.AsNoTracking();
    }
}