using Npgsql;
using Testcontainers.PostgreSql;

namespace Faryma.Composer.Application.Test.Infrastructure
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

        /// <summary>
        /// Возвращает хост временной базы данных.
        /// </summary>
        public string Host => _container.Hostname;

        /// <summary>
        /// Возвращает проброшенный порт PostgreSQL-контейнера.
        /// </summary>
        public int Port => _container.GetMappedPublicPort(PostgreSqlBuilder.PostgreSqlPort);

        /// <summary>
        /// Возвращает имя пользователя для подключения к тестовой базе.
        /// </summary>
        public string Username => PostgreSqlBuilder.DefaultUsername;

        /// <summary>
        /// Возвращает пароль для подключения к тестовой базе.
        /// </summary>
        public string Password => PostgreSqlBuilder.DefaultPassword;

        /// <summary>
        /// Запускает контейнер перед выполнением набора тестов.
        /// </summary>
        public Task InitializeAsync() => _container.StartAsync();

        /// <summary>
        /// Останавливает контейнер после завершения набора тестов.
        /// </summary>
        public Task DisposeAsync() => _container.DisposeAsync().AsTask();

        /// <summary>
        /// Создает отдельную базу данных для изолированной проверки.
        /// </summary>
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
    }
}
