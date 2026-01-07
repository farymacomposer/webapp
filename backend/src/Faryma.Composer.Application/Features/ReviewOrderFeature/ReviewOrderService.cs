using Faryma.Composer.Application.Features.AppSettings;
using Faryma.Composer.Application.Features.OrderQueueFeature;
using Faryma.Composer.Application.Features.OrderQueueFeature.Enums;
using Faryma.Composer.Application.Features.OrderQueueFeature.Models;
using Faryma.Composer.Application.Features.ReviewOrderFeature.Commands;
using Faryma.Composer.Application.Features.UserNicknameFeature;
using Faryma.Composer.Infrastructure;
using Faryma.Composer.Infrastructure.Entities;
using Faryma.Composer.Infrastructure.Enums;

namespace Faryma.Composer.Application.Features.ReviewOrderFeature
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
            ComposerStreamEntity? nearestStream = await uow.ComposerStream_RW.FindNearest(today)
                ?? throw new ReviewOrderException("Заказ не создан. Нет доступного стрима.");

            ReviewOrderEntity order = uow.ReviewOrder_RW.CreateFree(
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

        public async Task<ReviewOrderEntity> CreateDonation(CreateDonationOrderCommand command)
        {
            UserNicknameEntity userNickname = await userNicknameService.GetOrCreate(command.Nickname);
            ComposerStreamEntity nearestStream = await FindNearestStream(userNickname)
                ?? throw new ReviewOrderException("Заказ не создан. Нет доступного стрима.");

            TransactionEntity deposit = uow.Transaction_RW.CreateDeposit(userNickname.Account, command.PaymentAmount);
            TransactionEntity payment = uow.Transaction_RW.CreatePayment(userNickname.Account, command.PaymentAmount);

            ReviewOrderEntity order = uow.ReviewOrder_RW.CreateDonation(
                nearestStream,
                payment,
                appSettingsService.Settings.ReviewOrderNominalAmount,
                command.TrackUrl,
                command.UserComment);

            await uow.SaveChangesAsync();

            await orderQueueService.UpdateOrder(order, OrderQueueUpdateType.OrderCreated);

            return order;
        }

        public async Task<ReviewOrderEntity> CreateFree(CreateFreeOrderCommand command)
        {
            UserNicknameEntity userNickname = await userNicknameService.GetOrCreate(command.Nickname);
            ComposerStreamEntity nearestStream = await FindNearestStream(userNickname)
                ?? throw new ReviewOrderException("Заказ не создан. Нет доступного стрима.");

            ReviewOrderEntity order = uow.ReviewOrder_RW.CreateFree(
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

        public async Task<ReviewOrderEntity> CreateCharity(CreateCharityOrderCommand command)
        {
            ComposerStreamEntity? liveStream = await uow.ComposerStream_RW.FindLive();
            if (liveStream is null || liveStream.Type != ComposerStreamType.Charity)
            {
                throw new ReviewOrderException("Заказ не создан. Не запущен благотворительный стрим.");
            }

            UserNicknameEntity userNickname = await userNicknameService.GetOrCreate(command.Nickname);

            ReviewOrderEntity order = uow.ReviewOrder_RW.CreateFree(
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

        public async Task<TransactionEntity> MoveUp(MoveUpCommand command)
        {
            ReviewOrderEntity order = await uow.ReviewOrder_RW.Get(command.ReviewOrderId);

            if (order.Status is not (ReviewOrderStatus.Preorder or ReviewOrderStatus.Pending))
            {
                throw new ReviewOrderException("Невозможно поднять заказ", order);
            }

            UserNicknameEntity userNickname = await userNicknameService.GetOrCreate(command.Nickname);
            TransactionEntity deposit = uow.Transaction_RW.CreateDeposit(userNickname.Account, command.PaymentAmount);
            TransactionEntity payment = uow.Transaction_RW.CreatePayment(userNickname.Account, command.PaymentAmount);
            order.Payments.Add(payment);

            await uow.SaveChangesAsync();

            await orderQueueService.UpdateOrder(order, OrderQueueUpdateType.OrderMovedUp);

            return payment;
        }

        public async Task<ReviewOrderEntity> AddTrackUrl(AddTrackUrlCommand command)
        {
            ReviewOrderEntity order = await uow.ReviewOrder_RW.Get(command.ReviewOrderId);

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
            ReviewOrderEntity order = await uow.ReviewOrder_RW.Get(reviewOrderId);
            if (order.Status == ReviewOrderStatus.InProgress)
            {
                return order;
            }

            if (order.IsFrozen || order.Status != ReviewOrderStatus.Pending)
            {
                throw new ReviewOrderException("Невозможно взять в работу заказ", order);
            }

            ComposerStreamEntity liveStream = await uow.ComposerStream_RW.FindLive()
                ?? throw new ReviewOrderException("Невозможно взять в работу заказ вне активного стрима", order);

            ReviewOrderEntity? inProgress = await uow.ReviewOrder_R.FindInProgress();
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
            ReviewOrderEntity order = await uow.ReviewOrder_RW.Get(command.ReviewOrderId);
            if (order.Status == ReviewOrderStatus.Completed)
            {
                return order;
            }

            if (order.Status != ReviewOrderStatus.InProgress)
            {
                throw new ReviewOrderException("Невозможно выполнить заказ", order);
            }

            DateTime now = DateTime.UtcNow;

            order.Review = uow.Review_RW.Create(order, command.Rating, now);
            order.CompletedAt = now;
            order.Status = ReviewOrderStatus.Completed;

            await uow.SaveChangesAsync();

            await orderQueueService.UpdateOrder(order, OrderQueueUpdateType.OrderCompleted);

            return order;
        }

        public async Task<ReviewOrderEntity> Freeze(long reviewOrderId)
        {
            ReviewOrderEntity order = await uow.ReviewOrder_RW.Get(reviewOrderId);
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
            ReviewOrderEntity order = await uow.ReviewOrder_RW.Get(reviewOrderId);
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
            ReviewOrderEntity order = await uow.ReviewOrder_RW.Get(reviewOrderId);
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

            if (await uow.UserNickname_R.HasOrders(userNickname))
            {
                return await uow.ComposerStream_RW.FindNearest(today, ComposerStreamType.Donation);
            }
            else
            {
                return await uow.ComposerStream_RW.FindNearest(today);
            }
        }
    }
}