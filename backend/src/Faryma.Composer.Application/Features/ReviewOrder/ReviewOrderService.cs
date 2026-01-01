using Faryma.Composer.Application.Features.AppSettings;
using Faryma.Composer.Application.Features.OrderQueue;
using Faryma.Composer.Application.Features.UserNickname;
using Faryma.Composer.Contracts.Application.Features.OrderQueue.Enums;
using Faryma.Composer.Contracts.Application.Features.OrderQueue.Models;
using Faryma.Composer.Contracts.Application.Features.ReviewOrder;
using Faryma.Composer.Contracts.Application.Features.ReviewOrder.Commands;
using Faryma.Composer.Contracts.Infrastructure.Entities;
using Faryma.Composer.Contracts.Infrastructure.Entities.TransactionSources;
using Faryma.Composer.Contracts.Infrastructure.Enums;
using Faryma.Composer.Infrastructure;

namespace Faryma.Composer.Application.Features.ReviewOrder
{
    public sealed class ReviewOrderService(
        UnitOfWork uow,
        UserNicknameService userNicknameService,
        AppSettingsService appSettingsService,
        OrderQueueService orderQueueService)
    {
        public async Task<ReviewOrderEntity> CreateOutOfQueue(CreateOutOfQueueOrderCommand command)
        {
            UserNicknameEntity userNickname = await userNicknameService.GetOrCreate(command.Nickname);

            DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
            ComposerStreamEntity? nearestStream = await uow.ComposerStreamWrite.FindNearest(today)
                ?? throw new ReviewOrderException("Заказ не создан. Нет доступного стрима.");

            ReviewOrderEntity order = uow.ReviewOrderWrite.Create(
                DateTime.UtcNow,
                appSettingsService.Settings.ReviewOrderNominalAmount,
                payableAmount: 0,
                command.TrackUrl,
                command.UserComment,
                ReviewOrderType.OutOfQueue,
                nearestStream,
                userNickname);

            await uow.SaveChangesAsync();

            await orderQueueService.UpdateOrder(order, OrderQueueUpdateType.OrderCreated);

            return order;
        }

        public async Task<ReviewOrderEntity> CreateDonation(CreateDonationOrderCommand command)
        {
            UserNicknameEntity userNickname = await userNicknameService.GetOrCreate(command.Nickname);
            ComposerStreamEntity nearestStream = await FindNearestStream(userNickname)
                ?? throw new ReviewOrderException("Заказ не создан. Нет доступного стрима.");

            DateTime now = DateTime.UtcNow;

            TransactionEntity topUp = uow.TransactionWrite.CreateAccountTopUp(
                now,
                command.TopUpProvider,
                command.PaymentAmount,
                userNickname.Account);

            ReviewOrderEntity order = uow.ReviewOrderWrite.Create(
                now,
                appSettingsService.Settings.ReviewOrderNominalAmount,
                appSettingsService.Settings.ReviewOrderNominalAmount,
                command.TrackUrl,
                command.UserComment,
                ReviewOrderType.Donation,
                nearestStream,
                userNickname);

            TransactionEntity payment = uow.TransactionWrite.CreatePayment(
                now,
                command.PaymentAmount,
                userNickname.Account,
                order);

            order.Transactions.Add(payment);

            await uow.SaveChangesAsync();

            await orderQueueService.UpdateOrder(order, OrderQueueUpdateType.OrderCreated);

            return order;
        }

        public async Task<ReviewOrderEntity> CreateFree(CreateFreeOrderCommand command)
        {
            UserNicknameEntity userNickname = await userNicknameService.GetOrCreate(command.Nickname);
            ComposerStreamEntity nearestStream = await FindNearestStream(userNickname)
                ?? throw new ReviewOrderException("Заказ не создан. Нет доступного стрима.");

            ReviewOrderEntity order = uow.ReviewOrderWrite.Create(
                DateTime.UtcNow,
                appSettingsService.Settings.ReviewOrderNominalAmount,
                payableAmount: 0,
                command.TrackUrl,
                command.UserComment,
                ReviewOrderType.Free,
                nearestStream,
                userNickname);

            await uow.SaveChangesAsync();

            await orderQueueService.UpdateOrder(order, OrderQueueUpdateType.OrderCreated);

            return order;
        }

        public async Task<ReviewOrderEntity> CreateCharity(CreateCharityOrderCommand command)
        {
            ComposerStreamEntity? liveStream = await uow.ComposerStreamWrite.FindLive();
            if (liveStream is null || liveStream.Type != ComposerStreamType.Charity)
            {
                throw new ReviewOrderException("Заказ не создан. Не запущен благотворительный стрим.");
            }

            UserNicknameEntity userNickname = await userNicknameService.GetOrCreate(command.Nickname);

            ReviewOrderEntity order = uow.ReviewOrderWrite.Create(
                DateTime.UtcNow,
                appSettingsService.Settings.ReviewOrderNominalAmount,
                payableAmount: 0,
                command.TrackUrl,
                command.UserComment,
                ReviewOrderType.Charity,
                liveStream,
                userNickname);

            await uow.SaveChangesAsync();

            await orderQueueService.UpdateOrder(order, OrderQueueUpdateType.OrderCreated);

            return order;
        }

        public async Task<TransactionEntity> MoveUp(MoveUpCommand command)
        {
            ReviewOrderEntity order = await uow.ReviewOrderWrite.Get(command.ReviewOrderId);

            if (order.Status is not (ReviewOrderStatus.Preorder or ReviewOrderStatus.Pending))
            {
                throw new ReviewOrderException("Невозможно поднять заказ", order);
            }

            UserNicknameEntity userNickname = await userNicknameService.GetOrCreate(command.Nickname);

            DateTime now = DateTime.UtcNow;

            TransactionEntity topUp = uow.TransactionWrite.CreateAccountTopUp(
                now,
                command.TopUpProvider,
                command.PaymentAmount,
                userNickname.Account);

            TransactionEntity payment = uow.TransactionWrite.CreatePayment(
                now,
                command.PaymentAmount,
                userNickname.Account,
                order);

            order.Transactions.Add(payment);

            await uow.SaveChangesAsync();

            await orderQueueService.UpdateOrder(order, OrderQueueUpdateType.OrderMovedUp);

            return payment;
        }

        public async Task<ReviewOrderEntity> AddTrackUrl(AddTrackUrlCommand command)
        {
            ReviewOrderEntity order = await uow.ReviewOrderWrite.Get(command.ReviewOrderId);

            if (order.Status is not (ReviewOrderStatus.Preorder or ReviewOrderStatus.Pending or ReviewOrderStatus.InProgress))
            {
                throw new ReviewOrderException("Невозможно добавить/изменить ссылку на трек", order);
            }

            order.TrackUrl = command.TrackUrl;

            if (order.Status == ReviewOrderStatus.Preorder)
            {
                order.Status = ReviewOrderStatus.Pending;
            }

            await uow.SaveChangesAsync();

            await orderQueueService.UpdateOrder(order, OrderQueueUpdateType.TrackUrlAdded);

            return order;
        }

        public async Task<ReviewOrderEntity> TakeInProgress(long reviewOrderId)
        {
            ReviewOrderEntity order = await uow.ReviewOrderWrite.Get(reviewOrderId);
            if (order.Status == ReviewOrderStatus.InProgress)
            {
                return order;
            }

            if (order.IsFrozen || order.Status != ReviewOrderStatus.Pending)
            {
                throw new ReviewOrderException("Невозможно взять в работу заказ", order);
            }

            ComposerStreamEntity liveStream = await uow.ComposerStreamWrite.FindLive()
                ?? throw new ReviewOrderException("Невозможно взять в работу заказ вне активного стрима", order);

            ReviewOrderEntity? inProgress = await uow.ReviewOrderRead.FindInProgress();
            if (inProgress is not null && inProgress.Id != reviewOrderId)
            {
                throw new ReviewOrderException($"Невозможно взять в работу заказ, пока заказ Id: {inProgress.Id} находится в работе", order);
            }

            OrderQueuePosition position = await orderQueueService.GetCurrentQueuePosition(order);

            order.CategoryType = position.Category.Type;
            order.ProcessingStream = liveStream;
            order.Status = ReviewOrderStatus.InProgress;
            order.InProgressAt = DateTime.UtcNow;

            await uow.SaveChangesAsync();

            await orderQueueService.UpdateOrder(order, OrderQueueUpdateType.OrderTaken);

            return order;
        }

        public async Task<ReviewOrderEntity> Complete(CompleteCommand command)
        {
            ReviewOrderEntity order = await uow.ReviewOrderWrite.Get(command.ReviewOrderId);
            if (order.Status == ReviewOrderStatus.Completed)
            {
                return order;
            }

            if (order.Status != ReviewOrderStatus.InProgress)
            {
                throw new ReviewOrderException("Невозможно выполнить заказ", order);
            }

            DateTime now = DateTime.UtcNow;

            order.Review = uow.ReviewWrite.Create(order, command.Rating, now);
            order.CompletedAt = now;
            order.Status = ReviewOrderStatus.Completed;

            await uow.SaveChangesAsync();

            await orderQueueService.UpdateOrder(order, OrderQueueUpdateType.OrderCompleted);

            return order;
        }

        public async Task<ReviewOrderEntity> Freeze(long reviewOrderId)
        {
            ReviewOrderEntity order = await uow.ReviewOrderWrite.Get(reviewOrderId);
            if (order.IsFrozen)
            {
                return order;
            }

            if (order.Status is not (ReviewOrderStatus.Preorder or ReviewOrderStatus.Pending))
            {
                throw new ReviewOrderException("Невозможно заморозить заказ", order);
            }

            order.IsFrozen = true;

            await uow.SaveChangesAsync();

            await orderQueueService.UpdateOrder(order, OrderQueueUpdateType.OrderFrozen);

            return order;
        }

        public async Task<ReviewOrderEntity> Unfreeze(long reviewOrderId)
        {
            ReviewOrderEntity order = await uow.ReviewOrderWrite.Get(reviewOrderId);
            if (!order.IsFrozen)
            {
                return order;
            }

            if (order.Status is not (ReviewOrderStatus.Preorder or ReviewOrderStatus.Pending))
            {
                throw new ReviewOrderException("Невозможно разморозить заказ", order);
            }

            order.IsFrozen = false;

            await uow.SaveChangesAsync();

            await orderQueueService.UpdateOrder(order, OrderQueueUpdateType.OrderUnfrozen);

            return order;
        }

        public async Task<ReviewOrderEntity> Cancel(long reviewOrderId)
        {
            ReviewOrderEntity order = await uow.ReviewOrderWrite.Get(reviewOrderId);
            if (order.Status == ReviewOrderStatus.Canceled)
            {
                return order;
            }

            if (order.Status is not (ReviewOrderStatus.Preorder or ReviewOrderStatus.Pending or ReviewOrderStatus.InProgress))
            {
                throw new ReviewOrderException("Невозможно отменить заказ", order);
            }

            ReviewOrderStatus previousStatus = order.Status;
            order.Status = ReviewOrderStatus.Canceled;

            await uow.SaveChangesAsync();

            await orderQueueService.CancelOrder(order, previousStatus);

            return order;
        }

        private async Task<ComposerStreamEntity?> FindNearestStream(UserNicknameEntity userNickname)
        {
            DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);

            if (await uow.UserNicknameRead.HasOrders(userNickname))
            {
                return await uow.ComposerStreamWrite.FindNearest(today, ComposerStreamType.Donation);
            }
            else
            {
                return await uow.ComposerStreamWrite.FindNearest(today);
            }
        }
    }
}