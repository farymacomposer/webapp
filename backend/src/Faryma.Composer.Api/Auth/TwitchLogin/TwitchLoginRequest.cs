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
        /// PKCE code_verifier, если используется на фронтенде
        /// </summary>
        [StringLength(512, MinimumLength = 1)]
        public string? CodeVerifier { get; init; }
    }
}