namespace Faryma.Composer.Api.Test.Infrastructure.Auth
{
    internal sealed record BrowserUserAuthenticationState(
        Guid UserId,
        string UserName,
        string? TwitchUserId,
        string? TwitchLogin,
        IReadOnlyCollection<string> Roles);

    internal sealed class BrowserUserAuthenticationStateHolder
    {
        public BrowserUserAuthenticationState? State { get; set; }
    }
}
