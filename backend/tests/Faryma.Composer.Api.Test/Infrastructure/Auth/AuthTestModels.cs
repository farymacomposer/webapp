namespace Faryma.Composer.Api.Test.Infrastructure.Auth
{
    public static class TestAuthRoles
    {
        public const string Admin = "TestAuthAdmin";
    }

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
            Roles = [TestAuthRoles.Admin],
        };

        public TestAuthUserSeed Browser { get; init; } = new()
        {
            UserName = "test_browser_user",
            TwitchUserId = "test-browser-user-id",
            TwitchLogin = "test_browser_user",
        };
    }

    public sealed record SeededAuthUsers(SeededAuthUser Admin, SeededAuthUser Browser);

    public sealed record AdminBearerClientOptions
    {
        public TestAuthUserSeed User { get; init; } = new()
        {
            UserName = "test_composer_admin",
            Password = "TestComposerPass123!",
            Roles = [TestAuthRoles.Admin],
        };
    }

    public sealed record BrowserUserClientOptions
    {
        public TestAuthUserSeed User { get; init; } = new()
        {
            UserName = "test_browser_user",
            TwitchUserId = "test-browser-user-id",
            TwitchLogin = "test_browser_user",
        };
    }
}
