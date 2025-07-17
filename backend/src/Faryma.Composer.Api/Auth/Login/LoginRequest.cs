using System.ComponentModel.DataAnnotations;

namespace Faryma.Composer.Api.Auth.Login
{
    /// <summary>
    /// Запрос входа в систему
    /// </summary>
    public sealed class LoginRequest
    {
        /// <summary>
        /// Имя пользователя
        /// </summary>
        [Required]
        public required string UserName { get; set; }

        /// <summary>
        /// Пароль
        /// </summary>
        [Required]
        public required string Password { get; set; }
    }
}