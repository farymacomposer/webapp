using Faryma.Composer.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Faryma.Composer.Infrastructure.Features.Auth
{
    public sealed class RefreshTokenStore(AppDbContext appDbContext, DateTimeService dateTimeService)
    {
        public RefreshTokenEntity Create(string tokenHash, Guid familyId, int expiryInDays, UserEntity user)
        {
            DateTime now = dateTimeService.Now;

            return appDbContext.Add(new RefreshTokenEntity
            {
                TokenHash = tokenHash,
                FamilyId = familyId,
                CreatedAt = now,
                ExpiresAt = now.AddDays(expiryInDays),
                User = user,
            }).Entity;
        }

        public Task<RefreshTokenEntity?> FindByHash(string tokenHash) =>
            appDbContext.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == tokenHash);

        public Task<RefreshTokenEntity?> FindByUserIdAndHash(Guid userId, string tokenHash) =>
            appDbContext.RefreshTokens.FirstOrDefaultAsync(x => x.UserId == userId && x.TokenHash == tokenHash);

        public Task RevokeFamily(Guid familyId)
        {
            return appDbContext.RefreshTokens
                .Where(x => x.FamilyId == familyId && x.RevokedAt == null)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.RevokedAt, dateTimeService.Now));
        }

        public Task RevokeAllForUser(Guid userId)
        {
            return appDbContext.RefreshTokens
                .Where(x => x.UserId == userId && x.RevokedAt == null)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.RevokedAt, dateTimeService.Now));
        }
    }
}
