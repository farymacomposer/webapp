using Faryma.Composer.Contracts.Infrastructure.Entities.TransactionSources;
using Faryma.Composer.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Faryma.Composer.Application.Features.OrderQueue
{
    public sealed class OrderQueueBackgroundWorker(
        OrderQueueEventChannel eventChannel,
        IServiceScopeFactory scopeFactory,
        OrderQueueService orderQueueService,
        ILogger<OrderQueueBackgroundWorker> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            logger.LogInformation("OrderQueueBackgroundWorker started");

            while (!ct.IsCancellationRequested)
            {
                try
                {
                    await foreach (OrderQueueEvent evt in eventChannel.ReadAll(ct))
                    {
                        await HandleWithRetry(evt, ct);
                    }
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Ошибка цикла обработки событий очереди. Пересобираем состояние очереди.");
                    await SafeReinitialize(ct);
                }
            }

            logger.LogInformation("OrderQueueBackgroundWorker stopped");
        }

        private async Task HandleWithRetry(OrderQueueEvent evt, CancellationToken ct)
        {
            const int maxAttempts = 3;

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    await HandleEvent(evt, ct);
                    return;
                }
                catch (Exception ex) when (attempt < maxAttempts)
                {
                    TimeSpan delay = TimeSpan.FromMilliseconds(200 * attempt);
                    logger.LogWarning(ex,
                        "Ошибка обработки события {EventType}, попытка {Attempt}/{MaxAttempts}",
                        evt.GetType().Name, attempt, maxAttempts);

                    await Task.Delay(delay, ct);
                }
            }

            // Если дошли сюда — все ретраи исчерпаны
            logger.LogError("Событие {EventType} не обработано после ретраев", evt.GetType().Name);
            await SafeReinitialize(ct);
        }

        private async Task HandleEvent(OrderQueueEvent evt, CancellationToken ct)
        {
            await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
            UnitOfWork uow = scope.ServiceProvider.GetRequiredService<UnitOfWork>();

            switch (evt)
            {
                case ReviewOrderUpdatedEvent updated:
                {
                    ReviewOrderEntity order = await uow.ReviewOrderStore.FindById(updated.OrderId, ct)
                        ?? throw new InvalidOperationException($"Order {updated.OrderId} not found");

                    await orderQueueService.UpdateOrder(order, updated.UpdateType);
                    break;
                }

                case ReviewOrderCanceledEvent canceled:
                {
                    ReviewOrderEntity order = await uow.ReviewOrderStore.FindById(canceled.OrderId, ct)
                        ?? throw new InvalidOperationException($"Order {canceled.OrderId} not found");

                    await orderQueueService.CancelOrder(order, canceled.PreviousStatus);
                    break;
                }

                default:
                    throw new NotSupportedException($"Неизвестный тип события: {evt.GetType().Name}");
            }
        }

        private async Task SafeReinitialize(CancellationToken ct)
        {
            try
            {
                await orderQueueService.Initialize();
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Не удалось пересобрать состояние OrderQueueService");
                await Task.Delay(TimeSpan.FromSeconds(1), ct);
            }
        }
    }
}