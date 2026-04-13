using Npgsql;
using Testcontainers.PostgreSql;

namespace Faryma.Composer.Application.Test.Infrastructure
{
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

        public string GetConnectionString(string databaseName)
        {
            NpgsqlConnectionStringBuilder builder = new(_container.GetConnectionString())
            {
                Database = databaseName,
            };

            return builder.ConnectionString;
        }

        public async Task<string> CreateDatabaseAsync()
        {
            string databaseName = $"app_test_{Guid.NewGuid():N}";

            await using NpgsqlConnection connection = new(_container.GetConnectionString());
            await connection.OpenAsync();

            await using NpgsqlCommand command = connection.CreateCommand();
            command.CommandText = $"""CREATE DATABASE "{databaseName}";""";
            await command.ExecuteNonQueryAsync();

            return databaseName;
        }

        public async Task DeleteDatabaseAsync(string databaseName)
        {
            await using NpgsqlConnection connection = new(_container.GetConnectionString());
            await connection.OpenAsync();

            await ExecuteAsync(connection, $"""REVOKE CONNECT ON DATABASE "{databaseName}" FROM PUBLIC;""");
            await ExecuteAsync(connection,
                $"""
                SELECT pg_terminate_backend(pid)
                FROM pg_stat_activity
                WHERE datname = '{databaseName}'
                  AND pid <> pg_backend_pid();
                """);
            await ExecuteAsync(connection, $"""DROP DATABASE IF EXISTS "{databaseName}";""");
        }

        private static async Task ExecuteAsync(NpgsqlConnection connection, string commandText)
        {
            await using NpgsqlCommand command = connection.CreateCommand();
            command.CommandText = commandText;
            await command.ExecuteNonQueryAsync();
        }
    }
}