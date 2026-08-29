using System.Security.Claims;
using Faryma.Composer.Api.Common.Extensions;
using Faryma.Composer.Api.Features.Auth.Login;
using Faryma.Composer.Api.Features.Auth.Logout;
using Faryma.Composer.Api.Features.Auth.Options;
using Faryma.Composer.Api.Features.Auth.RefreshToken;
using Faryma.Composer.Api.Features.Auth.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace Faryma.Composer.Api.Features.Auth
{
    /// <summary>
    /// Аутентификация пользователей
    /// </summary>
    [ApiController]
    [Route("api/[controller]/[action]")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public sealed class AuthController(
        AuthTokenService authTokenService,
        IOptions<TwitchOptions> twitchOptions,
        AdminAuthService adminAuthService) : ControllerBase
    {
        /// <summary>
        /// Выполняет десктопную аутентификацию администратора и возвращает JWT токен
        /// </summary>
        [HttpPost("sessions/desktop-admin")]
        [EnableRateLimiting("auth-login")]
        public async Task<ActionResult<LoginResponse>> DesktopAdminLogin(LoginRequest request, CancellationToken ct)
        {
            AuthenticatedAdmin? admin = await adminAuthService.Authenticate(request, ct);
            if (admin is null)
            {
                return Unauthorized();
            }

            (string accessToken, string refreshToken) = await authTokenService.IssueForUser(admin.User, ct);

            return Ok(new LoginResponse { AccessToken = accessToken, RefreshToken = refreshToken });
        }

        /// <summary>
        /// Выполняет браузерную аутентификацию администратора через логин и пароль
        /// </summary>
        [HttpPost("sessions/browser-admin")]
        [EnableRateLimiting("auth-login")]
        public async Task<IActionResult> BrowserAdminLogin(LoginRequest request, CancellationToken ct)
        {
            AuthenticatedAdmin? admin = await adminAuthService.Authenticate(request, ct);
            if (admin is null)
            {
                return Unauthorized();
            }

            ClaimsPrincipal principal = adminAuthService.CreateBrowserPrincipal(admin);
            await HttpContext.SignInAsync(AppAuthenticationSchemes.BrowserCookieScheme, principal);

            return NoContent();
        }

        /// <summary>
        /// Инициирует вход пользователя через Twitch OIDC
        /// </summary>
        [HttpGet("oauth/twitch")]
        [EnableRateLimiting("auth-login")]
        public IActionResult BrowserLogin()
        {
            return Challenge(
                new AuthenticationProperties
                {
                    RedirectUri = twitchOptions.Value.LoginSuccessRedirectUri
                },
                AppAuthenticationSchemes.TwitchOidcScheme);
        }

        /// <summary>
        /// Выполняет локальный выход браузерной сессии
        /// </summary>
        [HttpPost("sessions/browser/logout")]
        [EnableRateLimiting("auth-login")]
        public async Task<IActionResult> BrowserLogout()
        {
            await HttpContext.SignOutAsync(AppAuthenticationSchemes.BrowserCookieScheme);

            return NoContent();
        }

        /// <summary>
        /// Обновляет access token
        /// </summary>
        [HttpPost("tokens/refresh")]
        [EnableRateLimiting("auth-login")]
        public async Task<ActionResult<RefreshTokenResponse>> RefreshToken(RefreshTokenRequest request)
        {
            (string accessToken, string refreshToken) = await authTokenService.Refresh(request.RefreshToken);

            return Ok(new RefreshTokenResponse { AccessToken = accessToken, RefreshToken = refreshToken });
        }

        /// <summary>
        /// Выполняет выход пользователя из системы
        /// </summary>
        [HttpPost("tokens/revoke")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [EnableRateLimiting("auth-login")]
        public async Task<IActionResult> Logout(LogoutRequest request)
        {
            Guid userId = User.GetUserId();
            await authTokenService.RevokeSession(userId, request.RefreshToken);

            return NoContent();
        }

        /// <summary>
        /// Выполняет выход пользователя из всех сессий
        /// </summary>
        [HttpPost("tokens/revoke-all")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [EnableRateLimiting("auth-login")]
        public async Task<IActionResult> LogoutAll()
        {
            Guid userId = User.GetUserId();
            await authTokenService.RevokeAll(userId);

            return NoContent();
        }
    }
}
