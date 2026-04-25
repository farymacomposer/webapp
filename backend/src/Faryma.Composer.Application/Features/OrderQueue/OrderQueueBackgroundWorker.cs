using Faryma.Composer.Contracts.Application.Features.OrderQueue.Events;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Faryma.Composer.Application.Features.OrderQueue
{
    public sealed class OrderQueueBackgroundWorker(
        OrderQueueEventChannel orderQueueEventChannel,
        OrderQueueService orderQueueService,
        ILogger<OrderQueueBackgroundWorker> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await foreach (OrderQueueEvent evt in orderQueueEventChannel.ReadAll(stoppingToken))
                    {
                        await orderQueueService.HandleEvent(evt);
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Ошибка цикла обработки событий очереди");
                }
            }
        }
    }
}
