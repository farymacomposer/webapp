using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;

namespace Faryma.Composer.Contracts.Api.Features.Auth.Options
{
    public sealed record TwitchOptions
    {
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
    }
}
