namespace Faryma.Composer.Api.Test.Infrastructure
{
    internal static class TestConfiguration
    {
        public const string JwtIssuer = "https://tests.faryma.local";
        public const string JwtAudience = "https://tests.faryma.local/api";
        public const string JwtSecretKey = "test-secret-key-with-enough-length-123456";

        public static IReadOnlyDictionary<string, string?> CreateNoDatabase()
        {
            Dictionary<string, string?> configuration = CreateBase();
            configuration.Add("POSTGRES:HOST", "localhost");
            configuration.Add("POSTGRES:PORT", "5432");
            configuration.Add("POSTGRES:DATABASE", "unused_api_test");
            configuration.Add("POSTGRES:USERNAME", "unused");
            configuration.Add("POSTGRES:PASSWORD", "unused-password");

            return configuration;
        }

        public static IReadOnlyDictionary<string, string?> CreatePostgreSql(PostgreSqlFixture fixture, string databaseName)
        {
            Dictionary<string, string?> configuration = new(PostgreSqlTestConfiguration.CreatePostgreSqlSettings(fixture, databaseName));
            foreach ((string key, string? value) in CreateBase())
            {
                configuration.Add(key, value);
            }

            return configuration;
        }

        private static Dictionary<string, string?> CreateBase()
        {
            return new Dictionary<string, string?>
            {
                ["JWT:ISSUER"] = JwtIssuer,
                ["JWT:AUDIENCE"] = JwtAudience,
                ["JWT:SECRET_KEY"] = JwtSecretKey,
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
