using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Faryma.Composer.Api.Test.Infrastructure
{
    /// <summary>
    /// Поднимает API test host с изолированной базой и тестовой конфигурацией.
    /// </summary>
    public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        private readonly IReadOnlyDictionary<string, string?> _configuration;
        private readonly Action<IWebHostBuilder>? _configureWebHost;
        private readonly List<CustomWebApplicationFactory> _ownedFactories = [];
        private bool _ownedFactoriesDisposed;

        public string DatabaseName { get; }

        private CustomWebApplicationFactory(
            IReadOnlyDictionary<string, string?> configuration,
            string databaseName,
            Action<IWebHostBuilder>? configureWebHost = null)
        {
            _configuration = configuration;
            DatabaseName = databaseName;
            _configureWebHost = configureWebHost;
        }

        public static async Task<CustomWebApplicationFactory> CreateAsync(PostgreSqlFixture fixture)
        {
            string databaseName = await fixture.CreateDatabaseAsync("api_test");
            IReadOnlyDictionary<string, string?> configuration = TestConfiguration.Create(fixture, databaseName);

            await EnsureDatabaseCreatedAsync(configuration);

            return new CustomWebApplicationFactory(configuration, databaseName);
        }

        public HttpClient CreateAnonymousClient() => CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false,
        });

        public override async ValueTask DisposeAsync()
        {
            await DisposeOwnedFactoriesAsync();
            await base.DisposeAsync();
        }

        internal CustomWebApplicationFactory CreateDerivedFactory(Action<IWebHostBuilder> configureWebHost)
        {
            ObjectDisposedException.ThrowIf(_ownedFactoriesDisposed, this);

            CustomWebApplicationFactory child = new(_configuration, DatabaseName, CombineConfigureActions(_configureWebHost, configureWebHost));
            _ownedFactories.Add(child);

            return child;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment(Environments.Development);

            builder.ConfigureTestServices(services =>
            {
                // Фоновый worker не нужен для smoke tests и добавляет лишнюю недетерминированность startup.
                services.RemoveAll<IHostedService>();
                services.AddControllers().AddApplicationPart(typeof(TestAuthProbeController).Assembly);
            });

            _configureWebHost?.Invoke(builder);
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((_, configurationBuilder) =>
                configurationBuilder.AddInMemoryCollection(_configuration));

            return base.CreateHost(builder);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeOwnedFactories();
            }

            base.Dispose(disposing);
        }

        private static async Task EnsureDatabaseCreatedAsync(IReadOnlyDictionary<string, string?> configuration)
        {
            ConfigurationManager configurationManager = new();
            configurationManager.AddInMemoryCollection(configuration);

            await PostgreSqlSchemaInitializer.EnsureCreatedAsync(configurationManager);
        }

        private static Action<IWebHostBuilder> CombineConfigureActions(
            Action<IWebHostBuilder>? first,
            Action<IWebHostBuilder> second)
        {
            return builder =>
            {
                first?.Invoke(builder);
                second(builder);
            };
        }

        private void DisposeOwnedFactories()
        {
            if (_ownedFactoriesDisposed)
            {
                return;
            }

            _ownedFactoriesDisposed = true;

            foreach (CustomWebApplicationFactory factory in _ownedFactories)
            {
                factory.Dispose();
            }

            _ownedFactories.Clear();
        }

        private async Task DisposeOwnedFactoriesAsync()
        {
            if (_ownedFactoriesDisposed)
            {
                return;
            }

            _ownedFactoriesDisposed = true;

            foreach (CustomWebApplicationFactory factory in _ownedFactories)
            {
                await factory.DisposeAsync();
            }

            _ownedFactories.Clear();
        }
    }
}
