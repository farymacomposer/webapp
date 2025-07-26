using Faryma.Composer.Core.Features.OrderQueueFeature.Contracts;
using Faryma.Composer.Core.Features.OrderQueueFeature.Models;
using Faryma.Composer.Core.Features.OrderQueueFeature.PriorityAlgorithm;
using Faryma.Composer.Infrastructure;
using Faryma.Composer.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Faryma.Composer.Core.Features.OrderQueueFeature
{
    public sealed class OrderQueueService(IDbContextFactory<AppDbContext> contextFactory, INotificationService notificationService)
    {
        private OrderQueueManager _queueManager = null!;

        public async Task Initialize()
        {
            await using AppDbContext context = await contextFactory.CreateDbContextAsync();

            _queueManager = await OrderQueueManager.CreateFromDatabase(context);

            _queueManager.UpdateOrderPositions();
        }

        public void Add(ReviewOrder order)
        {
            _queueManager.Add(order);
            _queueManager.UpdateOrderPositions();

            notificationService.SendOrderPosition(_queueManager.OrderPositionsById[order.Id]);
        }

        public void Up(Transaction payment)
        {
            _queueManager.Up(payment);
            _queueManager.UpdateOrderPositions();

            notificationService.SendOrderPosition(_queueManager.OrderPositionsById[payment.ReviewOrderId!.Value]);
        }

        public IReadOnlyCollection<OrderQueueItem> GetOrderQueue() => _queueManager.GetOrderQueue();
    }
}