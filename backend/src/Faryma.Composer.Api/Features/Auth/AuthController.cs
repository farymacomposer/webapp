using Faryma.Composer.Api.Common.Extensions;
using Faryma.Composer.Api.Features.Auth.Services;
using Faryma.Composer.Contracts.Api.Features.Auth.Login;
using Faryma.Composer.Contracts.Api.Features.Auth.Logout;
using Faryma.Composer.Contracts.Api.Features.Auth.Options;
using Faryma.Composer.Contracts.Api.Features.Auth.RefreshToken;
using Faryma.Composer.Contracts.Infrastructure.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace Faryma.Composer.Api.Features.Auth
{
    /// <summary>
    /// Аутентификация пользователей
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public sealed class AuthController(
        AuthTokenService authTokenService,
        IOptions<TwitchOptions> twitchOptions,
        UserManager<UserEntity> userManager) : ControllerBase
    {
        /// <summary>
        /// Выполняет аутентификацию пользователя и возвращает JWT токен
        /// </summary>
        [HttpPost(nameof(Login))]
        [EnableRateLimiting("auth-login")]
        public async Task<ActionResult<LoginResponse>> Login(LoginRequest request, CancellationToken ct)
        {
            UserEntity? user = await userManager.FindByNameAsync(request.UserName);
            if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
            {
                await Task.Delay(1000, ct);

                return Unauthorized();
            }

            (string accessToken, string refreshToken) = await authTokenService.IssueForUser(user, ct);

            return Ok(new LoginResponse { AccessToken = accessToken, RefreshToken = refreshToken });
        }

        /// <summary>
        /// Инициирует вход пользователя через Twitch OIDC
        /// </summary>
        [HttpGet("BrowserLogin")]
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
        [HttpPost("BrowserLogout")]
        [EnableRateLimiting("auth-login")]
        public async Task<IActionResult> BrowserLogout()
        {
            await HttpContext.SignOutAsync(AppAuthenticationSchemes.BrowserCookieScheme);
            return NoContent();
        }

        /// <summary>
        /// Обновляет access token
        /// </summary>
        [HttpPost(nameof(RefreshToken))]
        [EnableRateLimiting("auth-login")]
        public async Task<ActionResult<RefreshTokenResponse>> RefreshToken(RefreshTokenRequest request)
        {
            (string accessToken, string refreshToken) = await authTokenService.Refresh(request.RefreshToken);

            return Ok(new RefreshTokenResponse { AccessToken = accessToken, RefreshToken = refreshToken });
        }

        /// <summary>
        /// Выполняет выход пользователя из системы
        /// </summary>
        [HttpPost(nameof(Logout))]
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
        [HttpPost(nameof(LogoutAll))]
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
