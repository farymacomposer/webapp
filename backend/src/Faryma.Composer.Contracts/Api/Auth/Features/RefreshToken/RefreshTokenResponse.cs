using System.ComponentModel.DataAnnotations;

namespace Faryma.Composer.Contracts.Api.Auth.Features.RefreshToken
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