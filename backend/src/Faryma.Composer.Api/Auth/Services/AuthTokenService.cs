using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Authentication;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Faryma.Composer.Contracts.Api.Auth.Options;
using Faryma.Composer.Contracts.Infrastructure.Entities;
using Faryma.Composer.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Faryma.Composer.Api.Auth.Services
{
    public sealed class AuthTokenService(
        UnitOfWork uow,
        UserManager<UserEntity> userManager,
        IOptions<JwtOptions> options)
    {
        public async Task<(string AccessToken, string RefreshToken)> IssueForUser(UserEntity user, DateTime now, CancellationToken ct)
        {
            string accessToken = await GenerateAccessToken(user, now);
            string refreshToken = GenerateRefreshToken();

            uow.RefreshTokenStore.Create(
                tokenHash: Hash(refreshToken),
                familyId: Guid.NewGuid(),
                createdAt: now,
                options.Value.RefreshExpiryInDays,
                user);

            await uow.SaveChanges(ct);

            return (accessToken, refreshToken);
        }

        public async Task<(string AccessToken, string RefreshToken)> Refresh(string refreshToken, DateTime now, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                throw new AuthenticationException("Refresh token не передан");
            }

            string hash = Hash(refreshToken);
            RefreshTokenEntity stored = await uow.RefreshTokenStore.FindByHash(hash, ct)
                ?? throw new AuthenticationException("Refresh token не найден");

            if (stored.RevokedAt is not null)
            {
                if (!string.IsNullOrWhiteSpace(stored.ReplacedByTokenHash))
                {
                    await uow.RefreshTokenStore.RevokeFamily(stored.FamilyId, now, CancellationToken.None);
                }

                throw new AuthenticationException("Refresh token отозван");
            }

            if (stored.IsExpired(now))
            {
                stored.RevokedAt = now;
                await uow.SaveChanges(CancellationToken.None);

                throw new AuthenticationException("Refresh token истек");
            }

            UserEntity user = await uow.UserStore.FindById(stored.UserId, ct)
                ?? throw new AuthenticationException("Пользователь не найден");

            string nextRefresh = GenerateRefreshToken();
            string nextHash = Hash(nextRefresh);

            stored.RevokedAt = now;
            stored.ReplacedByTokenHash = nextHash;

            RefreshTokenEntity nextToken = uow.RefreshTokenStore.Create(
                nextHash,
                stored.FamilyId,
                createdAt: now,
                options.Value.RefreshExpiryInDays,
                user);

            string accessToken = await GenerateAccessToken(user, now);
            await uow.SaveChanges(ct);

            return (accessToken, nextRefresh);
        }

        public async Task RevokeSession(Guid userId, string refreshToken, DateTime now, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return;
            }

            string tokenHash = Hash(refreshToken);
            RefreshTokenEntity? stored = await uow.RefreshTokenStore.FindByUserIdAndHash(userId, tokenHash, ct);

            if (stored is null)
            {
                return;
            }

            await uow.RefreshTokenStore.RevokeFamily(stored.FamilyId, now, ct);
        }

        public Task RevokeAll(Guid userId, DateTime now, CancellationToken ct) => uow.RefreshTokenStore.RevokeAllForUser(userId, now, ct);

        private async Task<string> GenerateAccessToken(UserEntity user, DateTime now)
        {
            List<Claim> claims =
            [
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.UserName),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            ];

            IList<string> roles = await userManager.GetRolesAsync(user);
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(options.Value.SecretKey));

            JwtSecurityToken token = new(
                issuer: options.Value.Issuer,
                audience: options.Value.Audience,
                claims: claims,
                expires: now.AddMinutes(options.Value.ExpiryInMinutes),
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