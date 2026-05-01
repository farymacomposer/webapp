using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Faryma.Composer.Api.Test.Infrastructure.Auth
{
    internal sealed class BrowserUserTestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        BrowserUserAuthenticationStateHolder stateHolder)
        : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        public const string SchemeName = "TestBrowserUser";

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            BrowserUserAuthenticationState state = stateHolder.State
                ?? throw new InvalidOperationException("Состояние аутентификации браузерного пользователя не инициализировано");

            List<Claim> claims =
            [
                new(ClaimTypes.NameIdentifier, state.UserId.ToString()),
                new(ClaimTypes.Name, state.UserName),
            ];

            if (!string.IsNullOrWhiteSpace(state.TwitchUserId))
            {
                claims.Add(new Claim("twitch_user_id", state.TwitchUserId));
            }

            if (!string.IsNullOrWhiteSpace(state.TwitchLogin))
            {
                claims.Add(new Claim("preferred_username", state.TwitchLogin));
            }

            claims.AddRange(state.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

            ClaimsIdentity identity = new(claims, Scheme.Name);
            ClaimsPrincipal principal = new(identity);
            AuthenticationTicket ticket = new(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
