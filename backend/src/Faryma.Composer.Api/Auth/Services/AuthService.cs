using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Faryma.Composer.Contracts.Api.Auth.Options;
using Faryma.Composer.Contracts.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Faryma.Composer.Api.Auth.Services
{
    public sealed class AuthService(IOptions<JwtOptions> options, UserManager<UserEntity> userManager)
    {
        public async Task<string> GenerateJwtToken(UserEntity user, DateTime now)
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
    }
}