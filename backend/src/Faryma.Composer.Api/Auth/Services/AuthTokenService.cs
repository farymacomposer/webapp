using System.Security.Authentication;
using System.Security.Cryptography;
using System.Text;
using Faryma.Composer.Contracts.Api.Auth.Options;
using Faryma.Composer.Contracts.Infrastructure.Entities;
using Faryma.Composer.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;

namespace Faryma.Composer.Api.Auth.Services
{
    public sealed class AuthTokenService(
        AppDbContext dbContext,
        AuthService authService,
        UserManager<UserEntity> userManager,
        IOptions<JwtOptions> options)
    {
        public async Task<(string AccessToken, string RefreshToken)> IssueForUser(UserEntity user, DateTime now, CancellationToken cancellationToken)
        {
            string accessToken = await authService.GenerateJwtToken(user, now);
            string refreshToken = GenerateRefreshToken();

            dbContext.RefreshTokens.Add(new RefreshTokenEntity
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                User = user,
                TokenHash = ComputeSha256(refreshToken),
                FamilyId = Guid.NewGuid(),
                CreatedAt = now,
                ExpiresAt = now.AddDays(options.Value.RefreshExpiryInDays)
            });

            await dbContext.SaveChangesAsync(cancellationToken);

            return (accessToken, refreshToken);
        }

        public async Task<(string AccessToken, string RefreshToken)> Refresh(string refreshToken, DateTime now, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                throw new AuthenticationException("Refresh token не передан");
            }

            string tokenHash = ComputeSha256(refreshToken);
            await using IDbContextTransaction transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

            RefreshTokenEntity? storedToken = await dbContext.RefreshTokens
                .FirstOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken)
                ?? throw new AuthenticationException("Refresh token не найден");

            if (storedToken.RevokedAt is not null)
            {
                if (!string.IsNullOrWhiteSpace(storedToken.ReplacedByTokenHash))
                {
                    await RevokeFamily(storedToken.FamilyId, now, cancellationToken);
                }

                throw new AuthenticationException("Refresh token отозван");
            }

            if (storedToken.ExpiresAt <= now)
            {
                storedToken.RevokedAt = now;
                await dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                throw new AuthenticationException("Refresh token истек");
            }

            UserEntity user = await userManager.Users
                .FirstOrDefaultAsync(x => x.Id == storedToken.UserId, cancellationToken)
                ?? throw new AuthenticationException("Пользователь не найден");

            string nextRefreshToken = GenerateRefreshToken();
            string nextRefreshTokenHash = ComputeSha256(nextRefreshToken);
            storedToken.RevokedAt = now;
            storedToken.ReplacedByTokenHash = nextRefreshTokenHash;

            dbContext.RefreshTokens.Add(new RefreshTokenEntity
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                User = user,
                TokenHash = nextRefreshTokenHash,
                FamilyId = storedToken.FamilyId,
                CreatedAt = now,
                ExpiresAt = now.AddDays(options.Value.RefreshExpiryInDays)
            });

            string accessToken = await authService.GenerateJwtToken(user, now);
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return (accessToken, nextRefreshToken);
        }

        public async Task RevokeSession(Guid userId, string refreshToken, DateTime now, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return;
            }

            string tokenHash = ComputeSha256(refreshToken);
            RefreshTokenEntity? storedToken = await dbContext.RefreshTokens
                .FirstOrDefaultAsync(x => x.TokenHash == tokenHash && x.UserId == userId, cancellationToken);

            if (storedToken is null)
            {
                return;
            }

            await RevokeFamily(storedToken.FamilyId, now, cancellationToken);
        }

        public async Task RevokeAll(Guid userId, DateTime now, CancellationToken cancellationToken)
        {
            List<RefreshTokenEntity> activeTokens = await dbContext.RefreshTokens
                .Where(x => x.UserId == userId && x.RevokedAt == null)
                .ToListAsync(cancellationToken);

            if (activeTokens.Count == 0)
            {
                return;
            }

            foreach (RefreshTokenEntity token in activeTokens)
            {
                token.RevokedAt = now;
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        private static string GenerateRefreshToken()
        {
            Span<byte> randomBytes = stackalloc byte[64];
            RandomNumberGenerator.Fill(randomBytes);

            return Convert.ToBase64String(randomBytes)
                .Replace('+', '-')
                .Replace('/', '_')
                .TrimEnd('=');
        }

        private static string ComputeSha256(string value)
        {
            byte[] rawBytes = Encoding.UTF8.GetBytes(value);
            byte[] hash = SHA256.HashData(rawBytes);

            return Convert.ToHexString(hash);
        }

        private async Task RevokeFamily(Guid familyId, DateTime now, CancellationToken cancellationToken)
        {
            List<RefreshTokenEntity> activeFamilyTokens = await dbContext.RefreshTokens
                .Where(x => x.FamilyId == familyId && x.RevokedAt == null)
                .ToListAsync(cancellationToken);

            foreach (RefreshTokenEntity token in activeFamilyTokens)
            {
                token.RevokedAt = now;
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}