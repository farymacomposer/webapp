using Faryma.Composer.Core.Features.UserNicknameFeature;
using Faryma.Composer.Infrastructure;
using Faryma.Composer.Infrastructure.Entities;

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

        public IQueryable<Track> GetAll() => ofw.TrackRepository.GetAll();
    }
}