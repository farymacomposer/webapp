using System.ComponentModel.DataAnnotations;
using Faryma.Composer.Api.Features.Auth.Dtos;

namespace Faryma.Composer.Api.Features.Auth.RefreshToken
{
    /// <summary>
    /// Ответ на запрос обновления access token
    /// </summary>
    public sealed record RefreshTokenResponse
    {
        /// <summary>
        /// Новая пара JWT токенов
        /// </summary>
        [Required]
        public required AuthTokensDto Tokens { get; init; }
    }
}
