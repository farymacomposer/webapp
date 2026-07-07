using System.ComponentModel.DataAnnotations;

namespace Faryma.Composer.Api.Contracts.Features.Auth.Logout
{
    /// <summary>
    /// Запрос выхода из системы
    /// </summary>
    public sealed record LogoutRequest
    {
        [Required]
        [StringLength(512, MinimumLength = 1)]
        public required string RefreshToken { get; init; }
    }
}
