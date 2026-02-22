using System.ComponentModel.DataAnnotations;

namespace Faryma.Composer.Contracts.Api.Auth.Features.Login
{
    /// <summary>
    /// Ответ на запрос входа в систему
    /// </summary>
    public sealed record LoginResponse
    {
        /// <summary>
        /// JWT access token
        /// </summary>
        [Required]
        public required string Token { get; init; }

        /// <summary>
        /// Refresh token для продления сессии
        /// </summary>
        [Required]
        public required string RefreshToken { get; init; }
    }
}