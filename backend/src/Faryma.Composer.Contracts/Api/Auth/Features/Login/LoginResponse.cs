namespace Faryma.Composer.Contracts.Api.Auth.Features.Login
{
    /// <summary>
    /// Ответ на запрос входа в систему
    /// </summary>
    public sealed record LoginResponse
    {
        public required string AccessToken { get; init; }
        public required string RefreshToken { get; init; }
    }
}