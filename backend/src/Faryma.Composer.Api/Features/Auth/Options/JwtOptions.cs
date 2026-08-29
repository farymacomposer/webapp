using System.ComponentModel.DataAnnotations;

namespace Faryma.Composer.Api.Features.Auth.Options
{
    public sealed record JwtOptions
    {
        [ConfigurationKeyName("ISSUER")]
        [Required]
        [Url]
        public required string Issuer { get; init; }

        [ConfigurationKeyName("AUDIENCE")]
        [Required]
        [Url]
        public required string Audience { get; init; }

        [ConfigurationKeyName("SECRET_KEY")]
        [Required]
        [StringLength(256, MinimumLength = 36)]
        public required string SecretKey { get; init; }

        [ConfigurationKeyName("EXPIRY_IN_MINUTES")]
        [Range(1, 1440)]
        public required int ExpiryInMinutes { get; init; }

        [ConfigurationKeyName("REFRESH_EXPIRY_IN_DAYS")]
        [Range(1, 90)]
        public required int RefreshExpiryInDays { get; init; } = 14;
    }
}
