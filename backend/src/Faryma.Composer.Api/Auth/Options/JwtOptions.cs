using System.ComponentModel.DataAnnotations;

namespace Faryma.Composer.Api.Auth.Options
{
    public sealed record JwtOptions
    {
        [ConfigurationKeyName("ISSUER")]
        [Required]
        [Url]
        public required string Issuer { get; set; }

        [ConfigurationKeyName("AUDIENCE")]
        [Required]
        [Url]
        public required string Audience { get; set; }

        [ConfigurationKeyName("SECRET_KEY")]
        [Required]
        [MinLength(36)]
        public required string SecretKey { get; set; }

        [ConfigurationKeyName("EXPIRY_IN_MINUTES")]
        [Required]
        [Range(1, 1440)]
        public required int ExpiryInMinutes { get; set; }
    }
}