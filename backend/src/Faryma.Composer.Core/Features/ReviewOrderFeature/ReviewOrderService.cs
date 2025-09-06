using Faryma.Composer.Core.Features.AppSettings;
using Faryma.Composer.Core.Features.OrderQueueFeature;
using Faryma.Composer.Core.Features.OrderQueueFeature.Enums;
using Faryma.Composer.Core.Features.OrderQueueFeature.Models;
using Faryma.Composer.Core.Features.ReviewOrderFeature.Commands;
using Faryma.Composer.Core.Features.UserNicknameFeature;
using Faryma.Composer.Infrastructure;
using Faryma.Composer.Infrastructure.Entities;
using Faryma.Composer.Infrastructure.Enums;

namespace Faryma.Composer.Core.Features.ReviewOrderFeature
{
    public sealed class ReviewOrderService(
        UnitOfWork uow,
        UserNicknameService userNicknameService,
        AppSettingsService appSettingsService,
        OrderQueueService orderQueueService)
    {
        public async Task<ReviewOrder> CreateOutOfQueue(CreateOutOfQueueOrderCommand command)
        {
            UserNickname userNickname = await userNicknameService.GetOrCreate(command.Nickname);

            DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
            ComposerStream? nearestStream = await uow.ComposerStreamRepository.FindNearest(today)
                ?? throw new ReviewOrderException("Заказ не создан. Нет доступного стрима.");

            ReviewOrder order = uow.ReviewOrderRepository.CreateFree(
                nearestStream,
                userNickname,
                appSettingsService.Settings.ReviewOrderNominalAmount,
                command.TrackUrl,
                command.UserComment,
                ReviewOrderType.OutOfQueue);

            await uow.SaveChangesAsync();

            await orderQueueService.UpdateOrder(order, OrderQueueUpdateType.OrderCreated);

            return order;
        }

        public async Task<ReviewOrder> CreateDonation(CreateDonationOrderCommand command)
        {
            UserNickname userNickname = await userNicknameService.GetOrCreate(command.Nickname);
            ComposerStream nearestStream = await FindNearestStream(userNickname)
                ?? throw new ReviewOrderException("Заказ не создан. Нет доступного стрима.");

            Transaction deposit = uow.TransactionRepository.CreateDeposit(userNickname.Account, command.PaymentAmount);
            Transaction payment = uow.TransactionRepository.CreatePayment(userNickname.Account, command.PaymentAmount);

            ReviewOrder order = uow.ReviewOrderRepository.CreateDonation(
                nearestStream,
                payment,
                appSettingsService.Settings.ReviewOrderNominalAmount,
                command.TrackUrl,
                command.UserComment);

            await uow.SaveChangesAsync();

            await orderQueueService.UpdateOrder(order, OrderQueueUpdateType.OrderCreated);

            return order;
        }

        public async Task<ReviewOrder> CreateFree(CreateFreeOrderCommand command)
        {
            UserNickname userNickname = await userNicknameService.GetOrCreate(command.Nickname);
            ComposerStream nearestStream = await FindNearestStream(userNickname)
                ?? throw new ReviewOrderException("Заказ не создан. Нет доступного стрима.");

            ReviewOrder order = uow.ReviewOrderRepository.CreateFree(
                nearestStream,
                userNickname,
                appSettingsService.Settings.ReviewOrderNominalAmount,
                command.TrackUrl,
                command.UserComment,
                ReviewOrderType.Free);

            await uow.SaveChangesAsync();

            await orderQueueService.UpdateOrder(order, OrderQueueUpdateType.OrderCreated);

            return order;
        }

        public async Task<ReviewOrder> CreateCharity(CreateCharityOrderCommand command)
        {
            ComposerStream? liveStream = await uow.ComposerStreamRepository.FindLive();
            if (liveStream is null || liveStream.Type != ComposerStreamType.Charity)
            {
                throw new ReviewOrderException("Заказ не создан. Не запущен благотворительный стрим.");
            }

            UserNickname userNickname = await userNicknameService.GetOrCreate(command.Nickname);

            ReviewOrder order = uow.ReviewOrderRepository.CreateFree(
                liveStream,
                userNickname,
                appSettingsService.Settings.ReviewOrderNominalAmount,
                command.TrackUrl,
                command.UserComment,
                ReviewOrderType.Charity);

            await uow.SaveChangesAsync();

            await orderQueueService.UpdateOrder(order, OrderQueueUpdateType.OrderCreated);

            return order;
        }

        public async Task<Transaction> MoveUp(MoveUpCommand command)
        {
            ReviewOrder order = await uow.ReviewOrderRepository.Get(command.ReviewOrderId);

            if (order.Status is not (ReviewOrderStatus.Preorder or ReviewOrderStatus.Pending))
            {
                throw new ReviewOrderException("Невозможно поднять заказ", order);
            }

            UserNickname userNickname = await userNicknameService.GetOrCreate(command.Nickname);
            Transaction deposit = uow.TransactionRepository.CreateDeposit(userNickname.Account, command.PaymentAmount);
            Transaction payment = uow.TransactionRepository.CreatePayment(userNickname.Account, command.PaymentAmount);
            order.Payments.Add(payment);

            await uow.SaveChangesAsync();

            await orderQueueService.UpdateOrder(order, OrderQueueUpdateType.OrderMovedUp);

            return payment;
        }

        public async Task<ReviewOrder> AddTrackUrl(AddTrackUrlCommand command)
        {
            ReviewOrder order = await uow.ReviewOrderRepository.Get(command.ReviewOrderId);

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

        public async Task<ReviewOrder> TakeInProgress(long reviewOrderId)
        {
            ReviewOrder order = await uow.ReviewOrderRepository.Get(reviewOrderId);
            if (order.Status == ReviewOrderStatus.InProgress)
            {
                return order;
            }

            if (order.IsFrozen || order.Status != ReviewOrderStatus.Pending)
            {
                throw new ReviewOrderException("Невозможно взять в работу заказ", order);
            }

            ComposerStream liveStream = await uow.ComposerStreamRepository.FindLive()
                ?? throw new ReviewOrderException("Невозможно взять в работу заказ вне активного стрима", order);

            ReviewOrder? inProgress = await uow.ReviewOrderRepository.FindInProgress();
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

        public async Task<ReviewOrder> Complete(CompleteCommand command)
        {
            ReviewOrder order = await uow.ReviewOrderRepository.Get(command.ReviewOrderId);
            if (order.Status == ReviewOrderStatus.Completed)
            {
                return order;
            }

            if (order.Status != ReviewOrderStatus.InProgress)
            {
                throw new ReviewOrderException("Невозможно выполнить заказ", order);
            }

            DateTime now = DateTime.UtcNow;

            order.Review = uow.ReviewRepository.Create(order, command.Rating, now);
            order.CompletedAt = now;
            order.Status = ReviewOrderStatus.Completed;

            await uow.SaveChangesAsync();

            await orderQueueService.UpdateOrder(order, OrderQueueUpdateType.OrderCompleted);

            return order;
        }

        public async Task<ReviewOrder> Freeze(long reviewOrderId)
        {
            ReviewOrder order = await uow.ReviewOrderRepository.Get(reviewOrderId);
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

        public async Task<ReviewOrder> Unfreeze(long reviewOrderId)
        {
            ReviewOrder order = await uow.ReviewOrderRepository.Get(reviewOrderId);
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

        public async Task<ReviewOrder> Cancel(long reviewOrderId)
        {
            ReviewOrder order = await uow.ReviewOrderRepository.Get(reviewOrderId);
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

        private async Task<ComposerStream?> FindNearestStream(UserNickname userNickname)
        {
            DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);

            if (await uow.UserNicknameRepository.HasOrders(userNickname))
            {
                return await uow.ComposerStreamRepository.FindNearest(today, ComposerStreamType.Donation);
            }
            else
            {
                return await uow.ComposerStreamRepository.FindNearest(today);
            }
        }
    }
}