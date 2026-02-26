using Faryma.Composer.Contracts.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Faryma.Composer.Infrastructure.Persistence.Stores
{
    public sealed class RefreshTokenStore(AppDbContext context)
    {
        public Task<RefreshTokenEntity?> FindByHash(string tokenHash, CancellationToken ct) =>
            context.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == tokenHash, ct);

        public Task<RefreshTokenEntity?> FindByUserIdAndHash(Guid userId, string tokenHash, CancellationToken ct) =>
            context.RefreshTokens.FirstOrDefaultAsync(x => x.UserId == userId && x.TokenHash == tokenHash, ct);

        public RefreshTokenEntity Create(string tokenHash, Guid familyId, DateTime createdAt, int expiryInDays, UserEntity user)
        {
            return context.Add(new RefreshTokenEntity
            {
                TokenHash = tokenHash,
                FamilyId = familyId,
                CreatedAt = createdAt,
                ExpiresAt = createdAt.AddDays(expiryInDays),
                User = user,
            }).Entity;
        }

        public Task RevokeFamily(Guid familyId, DateTime now, CancellationToken ct)
        {
            return context.RefreshTokens
                .Where(x => x.FamilyId == familyId && x.RevokedAt == null)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.RevokedAt, now), ct);
        }

        public Task RevokeAllForUser(Guid userId, DateTime now, CancellationToken ct)
        {
            return context.RefreshTokens
                .Where(x => x.UserId == userId && x.RevokedAt == null)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.RevokedAt, now), ct);
        }
    }
}