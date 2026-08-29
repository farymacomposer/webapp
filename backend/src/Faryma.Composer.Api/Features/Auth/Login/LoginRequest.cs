using System.ComponentModel.DataAnnotations;

namespace Faryma.Composer.Api.Features.Auth.Login
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
        [StringLength(40, MinimumLength = 1)]
        public required string UserName { get; init; }

        /// <summary>
        /// Пароль
        /// </summary>
        [Required]
        [StringLength(40, MinimumLength = 12)]
        public required string Password { get; init; }
    }
}
