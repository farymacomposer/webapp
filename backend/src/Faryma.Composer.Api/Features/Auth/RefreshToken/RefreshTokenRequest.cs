using System.ComponentModel.DataAnnotations;

namespace Faryma.Composer.Api.Contracts.Features.Auth.RefreshToken
{
    /// <summary>
    /// Запрос обновления access token
    /// </summary>
    public sealed record RefreshTokenRequest
    {
        [Required]
        [StringLength(512, MinimumLength = 1)]
        public required string RefreshToken { get; init; }
    }
}
