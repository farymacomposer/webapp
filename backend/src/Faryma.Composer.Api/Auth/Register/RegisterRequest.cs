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
        [StringLength(40, MinimumLength = 1, ErrorMessage = "Длина имени должна быть в пределах от 1 до 40 символов")]
        public required string UserName { get; set; }

        /// <summary>
        /// Пароль
        /// </summary>
        [Required]
        [StringLength(40, MinimumLength = 12)]
        public required string Password { get; set; }
    }
}