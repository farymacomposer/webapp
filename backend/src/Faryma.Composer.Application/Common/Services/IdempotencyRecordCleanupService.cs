using Faryma.Composer.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Faryma.Composer.Application.Common.Services
{
    public sealed class IdempotencyRecordCleanupService(
        IServiceScopeFactory scopeFactory,
        ILogger<IdempotencyRecordCleanupService> logger) : BackgroundService
    {
        private static readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(1);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using PeriodicTimer timer = new(_cleanupInterval);

            while (!stoppingToken.IsCancellationRequested)
            {
                await DeleteExpiredRecords(stoppingToken);

                try
                {
                    await timer.WaitForNextTickAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    return;
                }
            }
        }

        private async Task DeleteExpiredRecords(CancellationToken ct)
        {
            try
            {
                using IServiceScope scope = scopeFactory.CreateScope();
                AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                DateTimeService dateTimeService = scope.ServiceProvider.GetRequiredService<DateTimeService>();

                int deleted = await context.IdempotencyRecords
                    .Where(x => x.ExpiresAt <= dateTimeService.Now)
                    .ExecuteDeleteAsync(ct);

                if (deleted > 0)
                {
                    logger.LogInformation("Удалено истекших записей идемпотентности: {Count}", deleted);
                }
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Не удалось удалить истекшие записи идемпотентности");
            }
        }
    }
}
