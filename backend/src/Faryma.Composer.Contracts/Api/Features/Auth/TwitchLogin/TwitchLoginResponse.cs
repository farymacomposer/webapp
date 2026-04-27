using System.ComponentModel.DataAnnotations;

namespace Faryma.Composer.Contracts.Api.Features.Auth.TwitchLogin
{
    /// <summary>
    /// Ответ на запрос входа в систему через Twitch OAuth
    /// </summary>
    public sealed record TwitchLoginResponse
    {
        [Required]
        public required string AccessToken { get; init; }

        [Required]
        public required string RefreshToken { get; init; }
    }
}
