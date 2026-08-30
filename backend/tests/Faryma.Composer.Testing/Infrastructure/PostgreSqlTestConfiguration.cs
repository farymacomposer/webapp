namespace Faryma.Composer.Testing.Infrastructure
{
    public static class PostgreSqlTestConfiguration
    {
        public static IReadOnlyDictionary<string, string?> CreatePostgreSqlSettings(
            PostgreSqlFixture fixture,
            string databaseName)
        {
            return new Dictionary<string, string?>
            {
                ["POSTGRES:HOST"] = fixture.Host,
                ["POSTGRES:PORT"] = fixture.Port.ToString(),
                ["POSTGRES:DATABASE"] = databaseName,
                ["POSTGRES:USERNAME"] = fixture.Username,
                ["POSTGRES:PASSWORD"] = fixture.Password,
                ["POSTGRES:POOLING"] = false.ToString(),
            };
        }
    }
}
