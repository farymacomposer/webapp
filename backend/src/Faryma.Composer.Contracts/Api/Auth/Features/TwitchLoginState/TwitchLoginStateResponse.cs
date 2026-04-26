using System.ComponentModel.DataAnnotations;

namespace Faryma.Composer.Contracts.Api.Auth.Features.TwitchLoginState
{
    /// <summary>
    /// Ответ на запрос выдачи state для Twitch OAuth
    /// </summary>
    public sealed record TwitchLoginStateResponse
    {
        /// <summary>
        /// OAuth state
        /// </summary>
        [Required]
        public required string State { get; init; }
    }
}
