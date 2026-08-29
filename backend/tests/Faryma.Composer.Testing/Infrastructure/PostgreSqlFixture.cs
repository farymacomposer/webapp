using Testcontainers.PostgreSql;

namespace Faryma.Composer.Testing.Infrastructure
{
    public sealed class PostgreSqlFixture : IAsyncLifetime
    {
        private const string _password = "postgres-test";

        private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:18-alpine")
            .WithDatabase(PostgreSqlBuilder.DefaultDatabase)
            .WithUsername(PostgreSqlBuilder.DefaultUsername)
            .WithPassword(_password)
            .Build();

        public string Host => _container.Hostname;
        public int Port => _container.GetMappedPublicPort(PostgreSqlBuilder.PostgreSqlPort);
        public string Username => PostgreSqlBuilder.DefaultUsername;
        public string Password => _password;

        public async ValueTask InitializeAsync() => await _container.StartAsync();
        public ValueTask DisposeAsync() => _container.DisposeAsync();

        public async Task<string> CreateDatabaseAsync(string prefix)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(prefix);

            string databaseName = $"{prefix}_{Guid.NewGuid():N}";
            await _container.ExecScriptAsync($"""CREATE DATABASE "{databaseName}";""");

            return databaseName;
        }
    }
}
