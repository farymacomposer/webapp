using System.ComponentModel.DataAnnotations;

namespace Faryma.Composer.Api.Auth.Options
{
    public sealed record TwitchOptions
    {
        [ConfigurationKeyName("CLIENT_ID")]
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public required string ClientId { get; init; }

        [ConfigurationKeyName("CLIENT_SECRET")]
        [Required]
        [StringLength(200, MinimumLength = 1)]
        public required string ClientSecret { get; init; }

        [ConfigurationKeyName("REDIRECT_URI")]
        [Required]
        [Url]
        public required string RedirectUri { get; init; }
    }
}