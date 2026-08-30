using Faryma.Composer.Application.Features.OrderQueue;
using Faryma.Composer.Application.Features.OrderQueue.Events;
using Faryma.Composer.Domain.Entities;
using Faryma.Composer.Domain.Entities.TransactionSources;
using Faryma.Composer.Infrastructure;
using Faryma.Composer.Infrastructure.Features.ComposerStream;
using Faryma.Composer.Infrastructure.Features.ReviewOrder;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Faryma.Composer.Testing.Infrastructure
{
    public static class TestServiceProviderExtensions
    {
        public static async Task<T> RunInScopeAsync<T>(this IServiceProvider services, Func<IServiceProvider, Task<T>> action)
        {
            await using AsyncServiceScope scope = services.CreateAsyncScope();

            return await action(scope.ServiceProvider);
        }

        public static async Task RunInScopeAsync(this IServiceProvider services, Func<IServiceProvider, Task> action)
        {
            await using AsyncServiceScope scope = services.CreateAsyncScope();
            await action(scope.ServiceProvider);
        }

        public static void SetCurrentUser(this IServiceProvider services, Guid userId) =>
            services.GetRequiredService<CurrentUserContext>().SetUserId(userId);

        public static async Task DrainOrderQueueEventsAsync(this IServiceProvider services)
        {
            OrderQueueEventChannel? channel = services.GetService<OrderQueueEventChannel>();
            OrderQueueService? orderQueueService = services.GetService<OrderQueueService>();
            if (channel is null || orderQueueService is null)
            {
                return;
            }

            while (channel.TryRead(out OrderQueueEvent? evt) && evt is not null)
            {
                await orderQueueService.HandleEvent(evt);
            }
        }

        public static TestOrderQueueNotificationService GetOrderQueueNotifications(this IServiceProvider services) =>
            (TestOrderQueueNotificationService)services.GetRequiredService<IOrderQueueNotificationService>();

        public static Task<ComposerStreamEntity> GetStreamAsync(
            this IServiceProvider services,
            long streamId,
            CancellationToken cancellationToken = default) =>
            services.RunInScopeAsync(scoped =>
                scoped.GetRequiredService<ComposerStreamStore>().GetStream(streamId, cancellationToken));

        public static Task<ReviewOrderEntity> GetOrderAsync(
            this IServiceProvider services,
            long orderId,
            CancellationToken cancellationToken = default) =>
            services.RunInScopeAsync(scoped =>
                scoped.GetRequiredService<ReviewOrderStore>().GetOrder(orderId, cancellationToken));

        public static Task<int> CountStreamsAsync(
            this IServiceProvider services,
            CancellationToken cancellationToken = default) =>
            services.RunInScopeAsync(async scoped =>
            {
                IDbContextFactory<AppDbContext> factory = scoped.GetRequiredService<IDbContextFactory<AppDbContext>>();
                await using AppDbContext context = await factory.CreateDbContextAsync(cancellationToken);

                return await context.ComposerStreams.CountAsync(cancellationToken);
            });

        public static Task<TResponse> Send<TResponse>(this IServiceProvider services, IRequest<TResponse> request) =>
            services.GetRequiredService<ISender>().Send(request).AsTask();
    }
}
