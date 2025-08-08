using Faryma.Composer.Api.Auth.Login;
using Faryma.Composer.Api.Auth.Register;
using Faryma.Composer.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Faryma.Composer.Api.Auth
{
    /// <summary>
    /// Регистрация и аутентификация пользователей
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public sealed class AuthController(AuthService authService, UserManager<User> userManager) : ControllerBase
    {
        /// <summary>
        /// Регистрирует нового пользователя в системе
        /// </summary>
        [HttpPost(nameof(Register))]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            User user = new()
            {
                CreatedAt = DateTime.UtcNow,
                UserName = request.UserName,
            };

            IdentityResult result = await userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            await userManager.AddToRoleAsync(user, "Composer");

            return Ok();
        }

        /// <summary>
        /// Выполняет аутентификацию пользователя и возвращает JWT токен
        /// </summary>
        [HttpPost(nameof(Login))]
        public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
        {
            User? user = await userManager.FindByNameAsync(request.UserName);
            if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
            {
                await Task.Delay(1000);

                return Unauthorized();
            }

            string token = await authService.GenerateJwtToken(user);

            return Ok(new LoginResponse { Token = token });
        }
    }
}