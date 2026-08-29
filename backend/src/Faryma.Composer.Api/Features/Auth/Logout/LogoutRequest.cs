using System.ComponentModel.DataAnnotations;

namespace Faryma.Composer.Api.Features.Auth.Logout
{
    /// <summary>
    /// Запрос выхода из текущей сессии
    /// </summary>
    public sealed record LogoutRequest
    {
        /// <summary>
        /// Refresh token отзываемой сессии
        /// </summary>
        [Required]
        [StringLength(512, MinimumLength = 1)]
        public required string RefreshToken { get; init; }
    }
}
