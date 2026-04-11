using Faryma.Composer.Contracts.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Faryma.Composer.Infrastructure.Persistence.Stores
{
    public sealed class RefreshTokenStore(AppDbContext context, DateTimeService dateTimeService)
    {
        public Task<RefreshTokenEntity?> FindByHash(string tokenHash, CancellationToken ct) =>
            context.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == tokenHash, ct);

        public Task<RefreshTokenEntity?> FindByUserIdAndHash(Guid userId, string tokenHash, CancellationToken ct) =>
            context.RefreshTokens.FirstOrDefaultAsync(x => x.UserId == userId && x.TokenHash == tokenHash, ct);

        public RefreshTokenEntity Create(string tokenHash, Guid familyId, int expiryInDays, UserEntity user)
        {
            DateTime now = dateTimeService.Now;

            return context.Add(new RefreshTokenEntity
            {
                TokenHash = tokenHash,
                FamilyId = familyId,
                CreatedAt = now,
                ExpiresAt = now.AddDays(expiryInDays),
                User = user,
            }).Entity;
        }

        public Task RevokeFamily(Guid familyId, CancellationToken ct)
        {
            return context.RefreshTokens
                .Where(x => x.FamilyId == familyId && x.RevokedAt == null)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.RevokedAt, dateTimeService.Now), ct);
        }

        public Task RevokeAllForUser(Guid userId, CancellationToken ct)
        {
            return context.RefreshTokens
                .Where(x => x.UserId == userId && x.RevokedAt == null)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.RevokedAt, dateTimeService.Now), ct);
        }
    }
}