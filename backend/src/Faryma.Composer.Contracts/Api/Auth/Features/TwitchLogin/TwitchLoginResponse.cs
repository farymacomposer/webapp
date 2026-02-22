using System.ComponentModel.DataAnnotations;

namespace Faryma.Composer.Contracts.Api.Auth.Features.TwitchLogin
{
    /// <summary>
    /// Ответ на запрос входа в систему через Twitch OAuth
    /// </summary>
    public sealed record TwitchLoginResponse
    {
        /// <summary>
        /// JWT access token
        /// </summary>
        [Required]
        public required string Token { get; init; }

        /// <summary>
        /// Refresh token для продления сессии
        /// </summary>
        [Required]
        public required string RefreshToken { get; init; }
    }
}