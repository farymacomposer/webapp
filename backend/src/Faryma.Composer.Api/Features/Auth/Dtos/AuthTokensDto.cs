using System.ComponentModel.DataAnnotations;

namespace Faryma.Composer.Api.Features.Auth.Dtos
{
    /// <summary>
    /// Пара JWT токенов
    /// </summary>
    public sealed record AuthTokensDto
    {
        /// <summary>
        /// Access token
        /// </summary>
        [Required]
        public required string AccessToken { get; init; }

        /// <summary>
        /// Refresh token
        /// </summary>
        [Required]
        public required string RefreshToken { get; init; }
    }
}
