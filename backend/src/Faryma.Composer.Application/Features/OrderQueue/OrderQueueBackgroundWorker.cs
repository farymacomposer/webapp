using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Faryma.Composer.Application.Features.OrderQueue
{
    public sealed class OrderQueueBackgroundWorker(
        OrderQueueEventChannel orderQueueEventChannel,
        OrderQueueService orderQueueService,
        ILogger<OrderQueueBackgroundWorker> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    await foreach (OrderQueueEvent evt in orderQueueEventChannel.ReadAll(ct))
                    {
                        await orderQueueService.HandleEvent(evt);
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