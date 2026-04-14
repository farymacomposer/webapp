using Faryma.Composer.Application.DependencyInjection;
using Faryma.Composer.Application.Features.AppSettings;
using Faryma.Composer.Application.Features.OrderQueue;
using Faryma.Composer.Contracts.Application.Features.OrderQueue;
using Faryma.Composer.Contracts.Application.Features.OrderQueue.Events;
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
        private readonly IHost _host;
        private readonly OrderQueueService _orderQueueService;
        private readonly OrderQueueEventChannel _orderQueueEventChannel;

        public DateTime FixedNow { get; }
        public DateOnly Today => DateOnly.FromDateTime(FixedNow);
        public TestOrderQueueNotificationService Notifications { get; }
        public TestDataBuilder Data { get; }
        public int QueueUpdateCount => Notifications.UpdateCount;

        private ApplicationTestHost(IHost host, DateTime fixedNow)
        {
            _host = host;
            FixedNow = fixedNow;
            Notifications = (TestOrderQueueNotificationService)_host.Services.GetRequiredService<IOrderQueueNotificationService>();
            _orderQueueService = _host.Services.GetRequiredService<OrderQueueService>();
            _orderQueueEventChannel = _host.Services.GetRequiredService<OrderQueueEventChannel>();
            Data = new TestDataBuilder(this);
        }

        public static async Task<ApplicationTestHost> CreateAsync(PostgreSqlFixture fixture)
        {
            string databaseName = await fixture.CreateDatabaseAsync();
            DateTime fixedNow = new(2030, 1, 10, 12, 0, 0, DateTimeKind.Utc);
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
                builder.Services.RemoveAll<IHostedService>();

                host = builder.Build();

                await EnsureDatabaseCreatedAsync(host);
                await host.StartAsync();
                await host.Services.GetRequiredService<AppSettingsService>().Initialize();
                await host.Services.GetRequiredService<OrderQueueService>().Initialize();

                return new ApplicationTestHost(host, fixedNow);
            }
            catch
            {
                host?.Dispose();

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

        public async Task DrainQueueEventsAsync()
        {
            while (_orderQueueEventChannel.TryRead(out OrderQueueEvent? evt) && evt is not null)
            {
                await _orderQueueService.HandleEvent(evt);
            }
        }

        public Task<ReviewOrderEntity> GetOrderAsync(long orderId) => RunScopeAsync(async services =>
        {
            UnitOfWork uow = services.GetRequiredService<UnitOfWork>();

            return await uow.ReviewOrderStore.FindById(orderId)
                ?? throw new InvalidOperationException($"Order {orderId} not found.");
        });

        public Task<ComposerStreamEntity> GetStreamAsync(long streamId) => RunScopeAsync(async services =>
        {
            UnitOfWork uow = services.GetRequiredService<UnitOfWork>();

            return await uow.ComposerStreamStore.FindById(streamId)
                ?? throw new InvalidOperationException($"Stream {streamId} not found.");
        });

        public Task<List<TransactionEntity>> GetOrderTransactionsAsync(long orderId) => RunScopeAsync(async services =>
        {
            IDbContextFactory<AppDbContext> factory = services.GetRequiredService<IDbContextFactory<AppDbContext>>();
            await using AppDbContext context = await factory.CreateDbContextAsync();

            return await context.Transactions
                .AsNoTracking()
                .Where(x => x.TransactionSourceId == orderId)
                .OrderBy(x => x.Id)
                .ToListAsync();
        });

        public Task<int> GetReviewCountAsync() => RunScopeAsync(async services =>
        {
            IDbContextFactory<AppDbContext> factory = services.GetRequiredService<IDbContextFactory<AppDbContext>>();
            await using AppDbContext context = await factory.CreateDbContextAsync();

            return await context.Reviews.CountAsync();
        });

        public async ValueTask DisposeAsync()
        {
            await _host.StopAsync();
            _host.Dispose();
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