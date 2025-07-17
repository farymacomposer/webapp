using Faryma.Composer.Core.Features.UserNicknameFeature;
using Faryma.Composer.Infrastructure;
using Faryma.Composer.Infrastructure.Entities;
using Faryma.Composer.Infrastructure.Enums;

namespace Faryma.Composer.Core.Features.TrackFeature
{
    public sealed class TrackService(UnitOfWork ofw, UserNicknameService userNicknameService)
    {
        public async Task<Track> Create(string nickname, string url)
        {
            UserNickname user = await userNicknameService.GetOrCreate(nickname);
            Track result = ofw.TrackRepository.Create(user, url);
            await ofw.SaveChangesAsync();

            return result;
        }
    }

    public sealed record CreateTrackCommand
    {
        public required string Url { get; init; }
        public string? Title { get; init; }
        public int? Year { get; init; }
        public TrackOrigin? Origin { get; init; }
        public ICollection<string> Artists { get; } = [];
        public ICollection<string> Genres { get; } = [];
    }
}