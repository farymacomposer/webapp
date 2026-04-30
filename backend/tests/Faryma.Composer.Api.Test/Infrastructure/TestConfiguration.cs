namespace Faryma.Composer.Api.Test.Infrastructure
{
    internal static class TestConfiguration
    {
        public static IReadOnlyDictionary<string, string?> Create(PostgreSqlFixture fixture, string databaseName)
        {
            return new Dictionary<string, string?>
            {
                ["POSTGRES:HOST"] = fixture.Host,
                ["POSTGRES:PORT"] = fixture.Port.ToString(),
                ["POSTGRES:DATABASE"] = databaseName,
                ["POSTGRES:USERNAME"] = fixture.Username,
                ["POSTGRES:PASSWORD"] = fixture.Password,

                ["JWT:ISSUER"] = "https://tests.faryma.local",
                ["JWT:AUDIENCE"] = "https://tests.faryma.local/api",
                ["JWT:SECRET_KEY"] = "test-secret-key-with-enough-length-123456",
                ["JWT:EXPIRY_IN_MINUTES"] = "60",
                ["JWT:REFRESH_EXPIRY_IN_DAYS"] = "14",

                ["TWITCH:CLIENT_ID"] = "test-twitch-client-id-1234567890",
                ["TWITCH:CLIENT_SECRET"] = "test-twitch-client-secret-1234567890",
                ["TWITCH:REDIRECT_URI"] = "https://localhost/signin-oidc",
                ["TWITCH:LOGIN_SUCCESS_REDIRECT_URI"] = "https://localhost/auth/success",
                ["TWITCH:LOGIN_FAILURE_REDIRECT_URI"] = "https://localhost/auth/failure",

                ["ADMIN_BOOTSTRAP:COMPOSER:USERNAME"] = "composer_test_admin",
                ["ADMIN_BOOTSTRAP:COMPOSER:PASSWORD"] = "ComposerPass123!",
                ["ADMIN_BOOTSTRAP:MODERATOR:USERNAME"] = "moderator_test_admin",
                ["ADMIN_BOOTSTRAP:MODERATOR:PASSWORD"] = "ModeratorPass123!",
            };
        }
    }
}
