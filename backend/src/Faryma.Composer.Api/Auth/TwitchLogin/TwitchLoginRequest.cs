using System.ComponentModel.DataAnnotations;

namespace Faryma.Composer.Api.Auth.TwitchLogin
{
    /// <summary>
    /// Запрос входа через Twitch OAuth
    /// </summary>
    public sealed record TwitchLoginRequest
    {
        /// <summary>
        /// Authorization code, полученный от Twitch OAuth
        /// </summary>
        [Required]
        [StringLength(4000, MinimumLength = 1)]
        public required string Code { get; init; }

        /// <summary>
        /// PKCE code_verifier
        /// </summary>
        [Required]
        [StringLength(512, MinimumLength = 1)]
        public required string CodeVerifier { get; init; }

        /// <summary>
        /// OAuth state, выданный backend перед редиректом на Twitch
        /// </summary>
        [Required]
        [StringLength(128, MinimumLength = 32)]
        public required string State { get; init; }
    }
}