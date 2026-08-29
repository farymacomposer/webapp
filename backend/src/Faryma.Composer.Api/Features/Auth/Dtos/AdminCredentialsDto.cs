using System.ComponentModel.DataAnnotations;

namespace Faryma.Composer.Api.Features.Auth.Dtos
{
    /// <summary>
    /// Учетные данные администратора
    /// </summary>
    public sealed record AdminCredentialsDto
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
