using System.ComponentModel.DataAnnotations;

namespace Faryma.Composer.Contracts.Api.Features.Auth.Login
{
    /// <summary>
    /// Ответ на запрос входа в систему
    /// </summary>
    public sealed record LoginResponse
    {
        [Required]
        public required string AccessToken { get; init; }

        [Required]
        public required string RefreshToken { get; init; }
    }
}
