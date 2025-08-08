using Faryma.Composer.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Faryma.Composer.Infrastructure.Repositories
{
    public sealed class TrackRepository(AppDbContext context)
    {
        public Task<Track?> Find(long id) => context.Tracks.FirstOrDefaultAsync(x => x.Id == id);

        public Track Create(UserNickname userNickname, string url)
        {
            return context.Add(new Track
            {
                UploadedAt = DateTime.UtcNow,
                UploadedBy = userNickname,
                Url = url,
            }).Entity;
        }

        public async Task<Track> GetOrCreate(UserNickname userNickname, string url)
        {
            Track? track = await context.Tracks.FirstOrDefaultAsync(x => x.Url == url);

            return track
                ?? context.Add(new Track
                {
                    UploadedAt = DateTime.UtcNow,
                    UploadedBy = userNickname,
                    Url = url,
                }).Entity;
        }
    }
}