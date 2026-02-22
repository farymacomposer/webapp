using System.ComponentModel.DataAnnotations;

namespace Faryma.Composer.Contracts.Api.Auth.Features.TwitchLogin
{
    /// <summary>
    /// Запрос входа в систему через Twitch OAuth
    /// </summary>
    public sealed record TwitchLoginRequest
    {
        /// <summary>
        /// Authorization code, полученный от Twitch OAuth
        /// </summary>
        [Required]
        [StringLength(30, MinimumLength = 30)]
        public required string Code { get; init; }

        /// <summary>
        /// PKCE code_verifier
        /// </summary>
        [Required]
        [StringLength(256, MinimumLength = 64)]
        public required string CodeVerifier { get; init; }

        /// <summary>
        /// OAuth state, выданный backend перед редиректом на Twitch
        /// </summary>
        [Required]
        [StringLength(128, MinimumLength = 32)]
        public required string State { get; init; }
    }
}