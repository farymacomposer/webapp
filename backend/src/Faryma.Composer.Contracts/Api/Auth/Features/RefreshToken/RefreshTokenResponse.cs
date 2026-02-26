namespace Faryma.Composer.Contracts.Api.Auth.Features.RefreshToken
{
    /// <summary>
    /// Ответ на запрос обновления access token
    /// </summary>
    public sealed record RefreshTokenResponse
    {
        public required string AccessToken { get; init; }
        public required string RefreshToken { get; init; }
    }
}