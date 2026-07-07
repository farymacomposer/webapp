using Faryma.Composer.Domain;

namespace Faryma.Composer.Api.Test.Infrastructure.Auth
{
    public sealed record TestAuthUserSeed
    {
        public required string UserName { get; init; }
        public string? Password { get; init; }
        public string? TwitchUserId { get; init; }
        public string? TwitchLogin { get; init; }
        public IReadOnlyCollection<string> Roles { get; init; } = [];
    }

    public sealed record SeededAuthUser(
        Guid UserId,
        string UserName,
        string? Password,
        string? TwitchUserId,
        string? TwitchLogin,
        IReadOnlyCollection<string> Roles);

    public sealed record AuthTestSeedOptions
    {
        public TestAuthUserSeed Admin { get; init; } = new()
        {
            UserName = "test_composer_admin",
            Password = "TestComposerPass123!",
            Roles = [AppRoles.Composer],
        };

        public TestAuthUserSeed Browser { get; init; } = new()
        {
            UserName = "test_browser_user",
            TwitchUserId = "test-browser-user-id",
            TwitchLogin = "test_browser_user",
        };
    }

    public sealed record SeededAuthUsers(SeededAuthUser Admin, SeededAuthUser Browser);
}
