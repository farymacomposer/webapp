using Faryma.Composer.Api.Auth.Services;
using Faryma.Composer.Api.Extensions;
using Faryma.Composer.Contracts.Api.Auth.Features.Login;
using Faryma.Composer.Contracts.Api.Auth.Features.Logout;
using Faryma.Composer.Contracts.Api.Auth.Features.RefreshToken;
using Faryma.Composer.Contracts.Api.Auth.Features.TwitchLogin;
using Faryma.Composer.Contracts.Api.Auth.Features.TwitchLoginState;
using Faryma.Composer.Contracts.Infrastructure.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Faryma.Composer.Api.Auth
{
    /// <summary>
    /// Аутентификация пользователей
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public sealed class AuthController(
        AuthTokenService authTokenService,
        TwitchAuthService twitchAuthService,
        TwitchAuthStateService twitchAuthStateService,
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
        /// Выдает state и nonce для Twitch OAuth
        /// </summary>
        [HttpGet(nameof(TwitchLoginState))]
        [EnableRateLimiting("auth-login")]
        public ActionResult<TwitchLoginStateResponse> TwitchLoginState()
        {
            (string state, string browserNonce) = twitchAuthStateService.IssueState();

            Response.Cookies.Append(
                TwitchAuthStateService.BrowserNonceCookieName,
                browserNonce,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    IsEssential = true,
                    MaxAge = TwitchAuthStateService.StateLifetime,
                    Path = "/api/Auth/TwitchLogin"
                });

            return Ok(new TwitchLoginStateResponse { State = state });
        }

        /// <summary>
        /// Выполняет вход пользователя через Twitch OAuth и возвращает JWT токен
        /// </summary>
        [HttpPost(nameof(TwitchLogin))]
        [EnableRateLimiting("auth-login")]
        public async Task<ActionResult<TwitchLoginResponse>> TwitchLogin(TwitchLoginRequest request, CancellationToken ct)
        {
            Request.Cookies.TryGetValue(TwitchAuthStateService.BrowserNonceCookieName, out string? browserNonce);
            Response.Cookies.Delete(TwitchAuthStateService.BrowserNonceCookieName, new CookieOptions
            {
                Path = "/api/Auth/TwitchLogin"
            });

            (string accessToken, string refreshToken) = await twitchAuthService.Login(
                request.Code,
                request.CodeVerifier,
                request.State,
                browserNonce,
                ct);

            return Ok(new TwitchLoginResponse { AccessToken = accessToken, RefreshToken = refreshToken });
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
        [Authorize]
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
        [Authorize]
        [EnableRateLimiting("auth-login")]
        public async Task<IActionResult> LogoutAll()
        {
            Guid userId = User.GetUserId();
            await authTokenService.RevokeAll(userId);

            return NoContent();
        }
    }
}
