using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;

namespace Faryma.Composer.Api.Contracts.Features.Auth.Options
{
    public sealed record TwitchOptions
    {
        public const string OidcAuthority = "https://id.twitch.tv/oauth2";
        public const string OidcMetadataAddress = OidcAuthority + "/.well-known/openid-configuration";

        [ConfigurationKeyName("CLIENT_ID")]
        [Required]
        [StringLength(128, MinimumLength = 30)]
        public required string ClientId { get; init; }

        [ConfigurationKeyName("CLIENT_SECRET")]
        [Required]
        [StringLength(128, MinimumLength = 30)]
        public required string ClientSecret { get; init; }

        [ConfigurationKeyName("REDIRECT_URI")]
        [Required]
        [Url]
        public required string RedirectUri { get; init; }

        [ConfigurationKeyName("LOGIN_SUCCESS_REDIRECT_URI")]
        [Required]
        [Url]
        public required string LoginSuccessRedirectUri { get; init; }

        [ConfigurationKeyName("LOGIN_FAILURE_REDIRECT_URI")]
        [Required]
        [Url]
        public required string LoginFailureRedirectUri { get; init; }
    }
}
