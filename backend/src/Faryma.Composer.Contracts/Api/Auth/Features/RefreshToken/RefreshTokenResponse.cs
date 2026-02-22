namespace Faryma.Composer.Contracts.Api.Auth.Features.RefreshToken
{
    /// <summary>
    /// Ответ на запрос обновления access token
    /// </summary>
    public sealed record RefreshTokenResponse
    {
        /// <summary>
        /// JWT access token
        /// </summary>
        public required string Token { get; init; }

        /// <summary>
        /// Refresh token для продления сессии
        /// </summary>
        public required string RefreshToken { get; init; }
    }
}