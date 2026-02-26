using System.ComponentModel.DataAnnotations;

namespace Faryma.Composer.Contracts.Api.Auth.Features.Login
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
        public required string UserName { get; init; }

        /// <summary>
        /// Пароль
        /// </summary>
        [Required]
        [StringLength(40, MinimumLength = 12, ErrorMessage = "Длина пароля должна быть в пределах от 12 до 40 символов")]
        public required string Password { get; init; }
    }
}