using Faryma.Composer.Application.DependencyInjection;
using Faryma.Composer.Application.Features.AppSettings;
using Faryma.Composer.Application.Features.OrderQueue;
using Faryma.Composer.Contracts.Application.Features.OrderQueue;
using Faryma.Composer.Contracts.Infrastructure.Entities;
using Faryma.Composer.Contracts.Infrastructure.Entities.TransactionSources;
using Faryma.Composer.Infrastructure.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Faryma.Composer.Application.Test.Infrastructure
{
    public sealed class ApplicationTestHost : IAsyncDisposable
    {
        private readonly PostgreSqlFixture _fixture;
        private readonly IHost _host;
        private readonly string _databaseName;

        public DateTime FixedNow { get; }
        public DateOnly Today => DateOnly.FromDateTime(FixedNow);
        public TestOrderQueueNotificationService Notifications { get; }
        public TestDataBuilder Data { get; }
        public int QueueUpdateCount => Notifications.UpdateCount;

        private ApplicationTestHost(
                                                    PostgreSqlFixture fixture,
            IHost host,
            string databaseName,
            DateTime fixedNow)
        {
            _fixture = fixture;
            _host = host;
            _databaseName = databaseName;
            FixedNow = fixedNow;
            Notifications = (TestOrderQueueNotificationService)_host.Services.GetRequiredService<IOrderQueueNotificationService>();
            Data = new TestDataBuilder(this);
        }

        public static async Task<ApplicationTestHost> CreateAsync(PostgreSqlFixture fixture, DateTime? now = null)
        {
            string databaseName = await fixture.CreateDatabaseAsync();
            DateTime fixedNow = now ?? new DateTime(2030, 1, 10, 12, 0, 0, DateTimeKind.Utc);
            IHost? host = null;

            try
            {
                HostApplicationBuilder builder = Host.CreateApplicationBuilder();
                builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["POSTGRES:HOST"] = fixture.Host,
                    ["POSTGRES:PORT"] = fixture.Port.ToString(),
                    ["POSTGRES:DATABASE"] = databaseName,
                    ["POSTGRES:USERNAME"] = fixture.Username,
                    ["POSTGRES:PASSWORD"] = fixture.Password,
                });

                builder.Services.AddSingleton<IOrderQueueNotificationService, TestOrderQueueNotificationService>();
                builder.Services.AddPersistence(builder.Configuration);
                builder.Services.RemoveAll<DateTimeService>();
                builder.Services.AddSingleton(new DateTimeService(fixedNow));
                builder.Services
                    .AddIdentityCore<UserEntity>()
                    .AddRoles<IdentityRole<Guid>>()
                    .AddEntityFrameworkStores<AppDbContext>();
                builder.Services.AddCoreServices();

                host = builder.Build();

                await EnsureDatabaseCreatedAsync(host);
                await host.StartAsync();
                await host.Services.GetRequiredService<AppSettingsService>().Initialize();
                await host.Services.GetRequiredService<OrderQueueService>().Initialize();

                return new ApplicationTestHost(fixture, host, databaseName, fixedNow);
            }
            catch
            {
                host?.Dispose();
                await fixture.DeleteDatabaseAsync(databaseName);
                throw;
            }
        }

        public async Task<T> RunScopeAsync<T>(Func<IServiceProvider, Task<T>> action)
        {
            await using AsyncServiceScope scope = _host.Services.CreateAsyncScope();
            return await action(scope.ServiceProvider);
        }

        public async Task RunScopeAsync(Func<IServiceProvider, Task> action)
        {
            await using AsyncServiceScope scope = _host.Services.CreateAsyncScope();
            await action(scope.ServiceProvider);
        }

        public Task WaitForQueueUpdateCountAsync(int expectedCount, TimeSpan? timeout = null) =>
            Notifications.WaitForCountAsync(expectedCount, timeout ?? TimeSpan.FromSeconds(5));

        public Task<ReviewOrderEntity> GetOrderAsync(long orderId) =>
            RunScopeAsync(async services =>
            {
                UnitOfWork uow = services.GetRequiredService<UnitOfWork>();
                return await uow.ReviewOrderStore.FindById(orderId, CancellationToken.None)
                    ?? throw new InvalidOperationException($"Order {orderId} not found.");
            });

        public Task<ComposerStreamEntity> GetStreamAsync(long streamId) =>
            RunScopeAsync(async services =>
            {
                UnitOfWork uow = services.GetRequiredService<UnitOfWork>();
                return await uow.ComposerStreamStore.FindById(streamId, CancellationToken.None)
                    ?? throw new InvalidOperationException($"Stream {streamId} not found.");
            });

        public Task<List<TransactionEntity>> GetOrderTransactionsAsync(long orderId)
        {
            return RunScopeAsync(async services =>
            {
                IDbContextFactory<AppDbContext> factory = services.GetRequiredService<IDbContextFactory<AppDbContext>>();
                await using AppDbContext context = await factory.CreateDbContextAsync();

                return await context.Transactions
                    .AsNoTracking()
                    .Where(x => x.TransactionSourceId == orderId)
                    .OrderBy(x => x.Id)
                    .ToListAsync();
            });
        }

        public Task<int> GetReviewCountAsync()
        {
            return RunScopeAsync(async services =>
            {
                IDbContextFactory<AppDbContext> factory = services.GetRequiredService<IDbContextFactory<AppDbContext>>();
                await using AppDbContext context = await factory.CreateDbContextAsync();
                return await context.Reviews.CountAsync();
            });
        }

        public async ValueTask DisposeAsync()
        {
            await _host.StopAsync();
            _host.Dispose();
            await _fixture.DeleteDatabaseAsync(_databaseName);
        }

        private static async Task EnsureDatabaseCreatedAsync(IHost host)
        {
            await using AsyncServiceScope scope = host.Services.CreateAsyncScope();
            IDbContextFactory<AppDbContext> factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
            await using AppDbContext context = await factory.CreateDbContextAsync();
            await context.Database.EnsureCreatedAsync();
        }
    }
}