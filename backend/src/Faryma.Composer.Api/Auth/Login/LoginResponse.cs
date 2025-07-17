using System.ComponentModel.DataAnnotations;

namespace Faryma.Composer.Api.Auth.Login
{
    /// <summary>
    /// Ответ на запрос входа в систему
    /// </summary>
    public sealed class LoginResponse
    {
        /// <summary>
        /// JWT-токен
        /// </summary>
        [Required]
        public required string Token { get; init; }
    }
}