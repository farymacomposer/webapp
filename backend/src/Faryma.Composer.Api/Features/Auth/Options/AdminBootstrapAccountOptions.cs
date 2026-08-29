using System.ComponentModel.DataAnnotations;

namespace Faryma.Composer.Api.Features.Auth.Options
{
    public sealed record AdminBootstrapAccountOptions
    {
        [ConfigurationKeyName("USERNAME")]
        [Required]
        [StringLength(40, MinimumLength = 1)]
        public required string UserName { get; init; }

        [ConfigurationKeyName("PASSWORD")]
        [Required]
        [StringLength(40, MinimumLength = 12)]
        public required string Password { get; init; }
    }
}
