using System.ComponentModel.DataAnnotations;

namespace Faryma.Composer.Api.Features.Auth.RefreshToken
{
    /// <summary>
    /// Ответ на запрос обновления access token
    /// </summary>
    public sealed record RefreshTokenResponse
    {
        [Required]
        public required string AccessToken { get; init; }

        [Required]
        public required string RefreshToken { get; init; }
    }
}
