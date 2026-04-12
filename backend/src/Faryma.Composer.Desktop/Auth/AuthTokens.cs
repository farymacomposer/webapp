namespace Faryma.Composer.Desktop.Auth
{
    public sealed record AuthTokens
    {
        public required string AccessToken { get; init; }
        public required string RefreshToken { get; init; }
    }
}