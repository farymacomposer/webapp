using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Authentication;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Faryma.Composer.Api.Features.Auth.Options;
using Faryma.Composer.Domain.Entities;
using Faryma.Composer.Infrastructure;
using Faryma.Composer.Infrastructure.Features.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Faryma.Composer.Api.Features.Auth.Services
{
    public sealed class AuthTokenService(
        AppDbContext appDbContext,
        RefreshTokenStore refreshTokenStore,
        UserManager<UserEntity> userManager,
        DateTimeContext dateTimeContext,
        IOptions<JwtOptions> options)
    {
        public async Task<(string AccessToken, string RefreshToken)> IssueForUser(UserEntity user, CancellationToken ct)
        {
            string accessToken = await GenerateAccessToken(user);
            string refreshToken = GenerateRefreshToken();

            refreshTokenStore.Create(
                tokenHash: Hash(refreshToken),
                familyId: Guid.NewGuid(),
                options.Value.RefreshExpiryInDays,
                user);

            await appDbContext.SaveChangesAsync(ct);

            return (accessToken, refreshToken);
        }

        public async Task<(string AccessToken, string RefreshToken)> Refresh(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                throw new AuthenticationException("Refresh token не передан");
            }

            string hash = Hash(refreshToken);
            RefreshTokenEntity stored = await refreshTokenStore.FindByHash(hash)
                ?? throw new AuthenticationException("Refresh token не найден");

            if (stored.RevokedAt is not null)
            {
                if (!string.IsNullOrWhiteSpace(stored.ReplacedByTokenHash))
                {
                    await refreshTokenStore.RevokeFamily(stored.FamilyId);
                }

                throw new AuthenticationException("Refresh token отозван");
            }

            if (stored.IsExpired(dateTimeContext.Now))
            {
                stored.RevokedAt = dateTimeContext.Now;
                try
                {
                    await appDbContext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException ex) when (ex.Entries.Any(entry => entry.Entity is RefreshTokenEntity token && token.Id == stored.Id))
                {
                    appDbContext.ChangeTracker.Clear();

                    RefreshTokenEntity? current = await refreshTokenStore.FindByHash(hash);
                    if (current is { RevokedAt: null })
                    {
                        throw;
                    }

                    if (!string.IsNullOrWhiteSpace(current?.ReplacedByTokenHash))
                    {
                        await refreshTokenStore.RevokeFamily(current.FamilyId);
                    }
                }

                throw new AuthenticationException("Refresh token истек");
            }

            UserEntity user = await userManager.Users.FirstOrDefaultAsync(x => x.Id == stored.UserId)
                ?? throw new AuthenticationException("Пользователь не найден");

            string nextRefresh = GenerateRefreshToken();
            string nextHash = Hash(nextRefresh);

            stored.RevokedAt = dateTimeContext.Now;
            stored.ReplacedByTokenHash = nextHash;

            RefreshTokenEntity nextToken = refreshTokenStore.Create(
                nextHash,
                stored.FamilyId,
                options.Value.RefreshExpiryInDays,
                user);

            string accessToken = await GenerateAccessToken(user);
            try
            {
                await appDbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex) when (ex.Entries.Any(entry => entry.Entity is RefreshTokenEntity token && token.Id == stored.Id))
            {
                Guid familyId = stored.FamilyId;
                appDbContext.ChangeTracker.Clear();

                RefreshTokenEntity? current = await refreshTokenStore.FindByHash(hash);
                if (current is not { RevokedAt: not null, ReplacedByTokenHash: not null })
                {
                    throw;
                }

                await refreshTokenStore.RevokeFamily(familyId);
                throw new AuthenticationException("Refresh token повторно использован");
            }

            return (accessToken, nextRefresh);
        }

        public async Task RevokeSession(Guid userId, string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return;
            }

            string tokenHash = Hash(refreshToken);
            RefreshTokenEntity? stored = await refreshTokenStore.FindByUserIdAndHash(userId, tokenHash);

            if (stored is null)
            {
                return;
            }

            await refreshTokenStore.RevokeFamily(stored.FamilyId);
        }

        public Task RevokeAll(Guid userId) => refreshTokenStore.RevokeAllForUser(userId);

        private async Task<string> GenerateAccessToken(UserEntity user)
        {
            List<Claim> claims =
            [
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.TwitchLogin ?? user.UserName ?? user.Id.ToString()),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            ];

            IList<string> roles = await userManager.GetRolesAsync(user);
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(options.Value.SecretKey));

            JwtSecurityToken token = new(
                issuer: options.Value.Issuer,
                audience: options.Value.Audience,
                claims: claims,
                expires: dateTimeContext.Now.AddMinutes(options.Value.ExpiryInMinutes),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            Span<byte> randomBytes = stackalloc byte[64];
            RandomNumberGenerator.Fill(randomBytes);

            return Convert.ToBase64String(randomBytes)
                .Replace('+', '-')
                .Replace('/', '_')
                .TrimEnd('=');
        }

        private string Hash(string refreshToken)
        {
            byte[] rawBytes = Encoding.UTF8.GetBytes(refreshToken);
            byte[] hash = SHA256.HashData(rawBytes);

            return Convert.ToHexString(hash);
        }
    }
}
