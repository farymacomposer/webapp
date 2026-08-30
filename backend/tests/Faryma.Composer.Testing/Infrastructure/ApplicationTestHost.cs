using Faryma.Composer.Application.DependencyInjection;
using Faryma.Composer.Application.Features.AppSettings;
using Faryma.Composer.Application.Features.OrderQueue;
using Faryma.Composer.Domain.Entities;
using Faryma.Composer.Domain.Entities.TransactionSources;
using Faryma.Composer.Infrastructure;
using Faryma.Composer.Infrastructure.DependencyInjection;
using Faryma.Composer.Infrastructure.Features.ReviewOrder;
using Mediator;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Faryma.Composer.Testing.Infrastructure
{
    /// <summary>
    /// Поднимает тестовый host приложения с изолированной базой и фиксированным временем.
    /// </summary>
    public sealed class ApplicationTestHost : IAsyncDisposable
    {
        private readonly IHost _host;

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
            Data = new TestDataBuilder(_host.Services);
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

                builder.Services.AddTestOrderQueueNotificationService();
                builder.Services.AddPersistence(builder.Configuration);
                builder.Services.AddFixedDateTimeContext(fixedNow);
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
        /// Отправляет команду или запрос через Mediator в отдельном DI scope.
        /// </summary>
        public Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request) =>
            RunScopeAsync(services => services.GetRequiredService<ISender>().Send(request).AsTask());

        /// <summary>
        /// Выполняет действие в отдельном DI scope и возвращает результат.
        /// </summary>
        public Task<T> RunScopeAsync<T>(Func<IServiceProvider, Task<T>> action) =>
            _host.Services.RunInScopeAsync(action);

        /// <summary>
        /// Выполняет действие в отдельном DI scope без возвращаемого значения.
        /// </summary>
        public Task RunScopeAsync(Func<IServiceProvider, Task> action) =>
            _host.Services.RunInScopeAsync(action);

        /// <summary>
        /// Обрабатывает накопленные события очереди перед проверкой результата.
        /// </summary>
        public Task DrainQueueEventsAsync() => _host.Services.DrainOrderQueueEventsAsync();

        /// <summary>
        /// Загружает заказ из базы для последующей проверки его состояния.
        /// </summary>
        public Task<ReviewOrderEntity> GetOrderAsync(long orderId) => RunScopeAsync(async services =>
        {
            ReviewOrderStore store = services.GetRequiredService<ReviewOrderStore>();

            return await store.FindOrderById(orderId, CancellationToken.None)
                ?? throw new InvalidOperationException($"Заказ {orderId} не найден");
        });

        /// <summary>
        /// Загружает стрим из базы для проверки сохраненных изменений.
        /// </summary>
        public Task<ComposerStreamEntity> GetStreamAsync(long streamId) =>
            _host.Services.GetStreamAsync(streamId);

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
