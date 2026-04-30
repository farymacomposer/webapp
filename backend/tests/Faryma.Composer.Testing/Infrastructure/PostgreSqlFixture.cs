using Testcontainers.PostgreSql;

namespace Faryma.Composer.Testing.Infrastructure
{
    /// <summary>
    /// Поднимает временный PostgreSQL-контейнер для интеграционных тестов.
    /// </summary>
    public sealed class PostgreSqlFixture : IAsyncLifetime
    {
        private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:18-alpine")
            .WithDatabase(PostgreSqlBuilder.DefaultDatabase)
            .WithUsername(PostgreSqlBuilder.DefaultUsername)
            .WithPassword(PostgreSqlBuilder.DefaultPassword)
            .Build();

        public string Host => _container.Hostname;

        public int Port => _container.GetMappedPublicPort(PostgreSqlBuilder.PostgreSqlPort);

        public string Username => PostgreSqlBuilder.DefaultUsername;

        public string Password => PostgreSqlBuilder.DefaultPassword;

        public Task InitializeAsync() => _container.StartAsync();

        public Task DisposeAsync() => _container.DisposeAsync().AsTask();

        public async Task<string> CreateDatabaseAsync(string prefix)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(prefix);

            string databaseName = $"{prefix}_{Guid.NewGuid():N}";
            await _container.ExecScriptAsync($"""CREATE DATABASE "{databaseName}";""");

            return databaseName;
        }
    }
}
