using Faryma.Composer.Infrastructure.Entities;

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
    }
}