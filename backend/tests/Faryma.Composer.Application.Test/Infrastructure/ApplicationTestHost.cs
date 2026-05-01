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
    /// <summary>
    /// Поднимает тестовый host приложения с изолированной базой и фиксированным временем.
    /// </summary>
    public sealed class ApplicationTestHost : IAsyncDisposable
    {
        private readonly IHost _host;
        private readonly OrderQueueService _orderQueueService;
        private readonly OrderQueueEventChannel _orderQueueEventChannel;

        /// <summary>
        /// Возвращает фиксированный момент времени, используемый в проверках.
        /// </summary>
        public DateTime FixedNow { get; }

        /// <summary>
        /// Возвращает тестовую дату, вычисленную из фиксированного времени.
        /// </summary>
        public DateOnly Today => DateOnly.FromDateTime(FixedNow);

        /// <summary>
        /// Дает доступ к фиктивным уведомлениям очереди для проверок.
        /// </summary>
        public TestOrderQueueNotificationService Notifications { get; }

        /// <summary>
        /// Создает тестовые сущности для сценариев проверки.
        /// </summary>
        public TestDataBuilder Data { get; }

        /// <summary>
        /// Показывает, сколько раз тест получил обновление очереди.
        /// </summary>
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

        /// <summary>
        /// Создает тестовый host на новой временной базе данных.
        /// </summary>
        public static async Task<ApplicationTestHost> CreateAsync(PostgreSqlFixture fixture)
        {
            string databaseName = await fixture.CreateDatabaseAsync("app_test");
            DateTime fixedNow = new(2030, 1, 10, 12, 0, 0, DateTimeKind.Utc);
            IHost? host = null;

            try
            {
                HostApplicationBuilder builder = Host.CreateApplicationBuilder();
                builder.Configuration.AddInMemoryCollection(PostgreSqlTestConfiguration.CreatePostgreSqlSettings(fixture, databaseName));

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

                await PostgreSqlSchemaInitializer.EnsureCreatedAsync(builder.Configuration);
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

        /// <summary>
        /// Выполняет действие в отдельном DI scope и возвращает результат.
        /// </summary>
        public async Task<T> RunScopeAsync<T>(Func<IServiceProvider, Task<T>> action)
        {
            await using AsyncServiceScope scope = _host.Services.CreateAsyncScope();

            return await action(scope.ServiceProvider);
        }

        /// <summary>
        /// Выполняет действие в отдельном DI scope без возвращаемого значения.
        /// </summary>
        public async Task RunScopeAsync(Func<IServiceProvider, Task> action)
        {
            await using AsyncServiceScope scope = _host.Services.CreateAsyncScope();
            await action(scope.ServiceProvider);
        }

        /// <summary>
        /// Обрабатывает накопленные события очереди перед проверкой результата.
        /// </summary>
        public async Task DrainQueueEventsAsync()
        {
            while (_orderQueueEventChannel.TryRead(out OrderQueueEvent? evt) && evt is not null)
            {
                await _orderQueueService.HandleEvent(evt);
            }
        }

        /// <summary>
        /// Загружает заказ из базы для последующей проверки его состояния.
        /// </summary>
        public Task<ReviewOrderEntity> GetOrderAsync(long orderId) => RunScopeAsync(async services =>
        {
            UnitOfWork uow = services.GetRequiredService<UnitOfWork>();

            return await uow.ReviewOrderStore.FindById(orderId)
                ?? throw new InvalidOperationException($"Order {orderId} not found.");
        });

        /// <summary>
        /// Загружает стрим из базы для проверки сохраненных изменений.
        /// </summary>
        public Task<ComposerStreamEntity> GetStreamAsync(long streamId) => RunScopeAsync(async services =>
        {
            UnitOfWork uow = services.GetRequiredService<UnitOfWork>();

            return await uow.ComposerStreamStore.FindById(streamId)
                ?? throw new InvalidOperationException($"Stream {streamId} not found.");
        });

        /// <summary>
        /// Возвращает транзакции заказа, чтобы проверить финансовые эффекты сценария.
        /// </summary>
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

        /// <summary>
        /// Считает созданные отзывы для проверки побочных эффектов.
        /// </summary>
        public Task<int> GetReviewCountAsync() => RunScopeAsync(async services =>
        {
            IDbContextFactory<AppDbContext> factory = services.GetRequiredService<IDbContextFactory<AppDbContext>>();
            await using AppDbContext context = await factory.CreateDbContextAsync();

            return await context.Reviews.CountAsync();
        });

        /// <summary>
        /// Останавливает тестовый host и освобождает его ресурсы.
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            await _host.StopAsync();
            _host.Dispose();
        }
    }
}
