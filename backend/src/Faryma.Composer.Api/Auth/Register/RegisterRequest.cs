using System.ComponentModel.DataAnnotations;

namespace Faryma.Composer.Api.Auth.Register
{
    /// <summary>
    /// Запрос регистрации пользователя
    /// </summary>
    public sealed record RegisterRequest
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