using Faryma.Composer.Api.Auth.Login;
using Faryma.Composer.Api.Auth.TwitchLogin;
using Faryma.Composer.Contracts.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Faryma.Composer.Api.Auth
{
    /// <summary>
    /// Аутентификация пользователей
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public sealed class AuthController(
        AuthService authService,
        TwitchAuthService twitchAuthService,
        UserManager<UserEntity> userManager) : ControllerBase
    {
        /// <summary>
        /// Выполняет аутентификацию пользователя и возвращает JWT токен
        /// </summary>
        [HttpPost(nameof(Login))]
        public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
        {
            UserEntity? user = await userManager.FindByNameAsync(request.UserName);
            if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
            {
                await Task.Delay(1000);

                return Unauthorized();
            }

            string token = await authService.GenerateJwtToken(user);

            return Ok(new LoginResponse { Token = token });
        }

        /// <summary>
        /// Выполняет вход пользователя через Twitch OAuth и возвращает JWT токен
        /// </summary>
        [HttpPost(nameof(TwitchLogin))]
        public async Task<ActionResult<LoginResponse>> TwitchLogin(TwitchLoginRequest request, CancellationToken cancellationToken)
        {
            string token = await twitchAuthService.Login(request.Code, request.CodeVerifier, cancellationToken);

            return Ok(new LoginResponse { Token = token });
        }
    }
}