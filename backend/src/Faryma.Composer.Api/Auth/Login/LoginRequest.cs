using System.ComponentModel.DataAnnotations;

namespace Faryma.Composer.Api.Auth.Login
{
    /// <summary>
    /// Запрос входа в систему
    /// </summary>
    public sealed record LoginRequest
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