namespace Faryma.Composer.Contracts.Api.Auth.Features.TwitchLogin
{
    /// <summary>
    /// Ответ на запрос входа в систему через Twitch OAuth
    /// </summary>
    public sealed record TwitchLoginResponse
    {
        public required string AccessToken { get; init; }
        public required string RefreshToken { get; init; }
    }
}