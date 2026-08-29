using System.Security.Claims;
using Faryma.Composer.Api.Features.Auth.BrowserAdminLogin;
using Faryma.Composer.Api.Features.Auth.DesktopAdminLogin;
using Faryma.Composer.Api.Features.Auth.Dtos;
using Faryma.Composer.Api.Features.Auth.Logout;
using Faryma.Composer.Api.Features.Auth.Options;
using Faryma.Composer.Api.Features.Auth.RefreshToken;
using Faryma.Composer.Api.Features.Auth.Services;
using Faryma.Composer.Infrastructure;
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
        AdminAuthService adminAuthService,
        CurrentUserContext currentUserContext) : ControllerBase
    {
        /// <summary>
        /// Выполняет десктопную аутентификацию администратора и возвращает JWT токен
        /// </summary>
        [HttpPost]
        [EnableRateLimiting("auth-login")]
        public async Task<ActionResult<DesktopAdminLoginResponse>> DesktopAdminLogin(DesktopAdminLoginRequest request, CancellationToken ct)
        {
            AuthenticatedAdmin? admin = await adminAuthService.Authenticate(request.Credentials.UserName, request.Credentials.Password, ct);
            if (admin is null)
            {
                return Unauthorized();
            }

            (string accessToken, string refreshToken) = await authTokenService.IssueForUser(admin.User, ct);

            return Ok(new DesktopAdminLoginResponse
            {
                Tokens = new AuthTokensDto
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken
                }
            });
        }

        /// <summary>
        /// Выполняет браузерную аутентификацию администратора через логин и пароль
        /// </summary>
        [HttpPost]
        [EnableRateLimiting("auth-login")]
        public async Task<IActionResult> BrowserAdminLogin(BrowserAdminLoginRequest request, CancellationToken ct)
        {
            AuthenticatedAdmin? admin = await adminAuthService.Authenticate(request.Credentials.UserName, request.Credentials.Password, ct);
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
        [HttpGet]
        [EnableRateLimiting("auth-login")]
        public IActionResult TwitchLogin()
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
        [HttpPost]
        [EnableRateLimiting("auth-login")]
        public async Task<IActionResult> BrowserLogout()
        {
            await HttpContext.SignOutAsync(AppAuthenticationSchemes.BrowserCookieScheme);

            return NoContent();
        }

        /// <summary>
        /// Обновляет access token
        /// </summary>
        [HttpPost]
        [EnableRateLimiting("auth-login")]
        public async Task<ActionResult<RefreshTokenResponse>> RefreshToken(RefreshTokenRequest request)
        {
            (string accessToken, string refreshToken) = await authTokenService.Refresh(request.RefreshToken);

            return Ok(new RefreshTokenResponse
            {
                Tokens = new AuthTokensDto
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken
                }
            });
        }

        /// <summary>
        /// Выполняет выход пользователя из системы
        /// </summary>
        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [EnableRateLimiting("auth-login")]
        public async Task<IActionResult> Logout(LogoutRequest request)
        {
            Guid userId = currentUserContext.GetRequiredUserId();
            await authTokenService.RevokeSession(userId, request.RefreshToken);

            return NoContent();
        }

        /// <summary>
        /// Выполняет выход пользователя из всех сессий
        /// </summary>
        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [EnableRateLimiting("auth-login")]
        public async Task<IActionResult> LogoutAll()
        {
            Guid userId = currentUserContext.GetRequiredUserId();
            await authTokenService.RevokeAll(userId);

            return NoContent();
        }
    }
}
