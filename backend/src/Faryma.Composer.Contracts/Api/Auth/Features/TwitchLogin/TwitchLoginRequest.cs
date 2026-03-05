using System.ComponentModel.DataAnnotations;

namespace Faryma.Composer.Contracts.Api.Auth.Features.TwitchLogin
{
    /// <summary>
    /// Запрос входа в систему через Twitch OAuth
    /// </summary>
    public sealed record TwitchLoginRequest : IValidatableObject
    {
        /// <summary>
        /// Authorization code, полученный от Twitch OAuth
        /// </summary>
        [Required]
        [StringLength(512, MinimumLength = 1)]
        public required string Code { get; init; }

        /// <summary>
        /// PKCE code_verifier
        /// </summary>
        [Required]
        [StringLength(128, MinimumLength = 43)]
        public required string CodeVerifier { get; init; }

        /// <summary>
        /// OAuth state, выданный backend перед редиректом на Twitch
        /// </summary>
        [Required]
        [StringLength(128, MinimumLength = 32)]
        public required string State { get; init; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (CodeVerifier.Any(static symbol => !(char.IsAsciiLetterOrDigit(symbol) || symbol is '-' or '.' or '_' or '~')))
            {
                yield return new ValidationResult("CodeVerifier содержит недопустимые символы");
            }
        }
    }
}