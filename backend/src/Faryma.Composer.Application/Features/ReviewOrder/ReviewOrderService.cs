using Faryma.Composer.Application.Features.AppSettings;
using Faryma.Composer.Application.Features.OrderQueue;
using Faryma.Composer.Application.Features.UserNickname;
using Faryma.Composer.Contracts.Application.Features.OrderQueue.Enums;
using Faryma.Composer.Contracts.Application.Features.OrderQueue.Events;
using Faryma.Composer.Contracts.Application.Features.OrderQueue.Models;
using Faryma.Composer.Contracts.Application.Features.ReviewOrder;
using Faryma.Composer.Contracts.Application.Features.ReviewOrder.Commands;
using Faryma.Composer.Contracts.Infrastructure.Entities;
using Faryma.Composer.Contracts.Infrastructure.Entities.TransactionSources;
using Faryma.Composer.Contracts.Infrastructure.Enums;
using Faryma.Composer.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Faryma.Composer.Application.Features.ReviewOrder
{
    public sealed class ReviewOrderService(
        UnitOfWork uow,
        UserManager<UserEntity> userManager,
        UserNicknameService userNicknameService,
        AppSettingsService appSettingsService,
        OrderQueueService orderQueueService,
        OrderQueueEventChannel orderQueueEventChannel)
    {
        public async Task<ReviewOrderEntity> CreateOutOfQueue(CreateOutOfQueueOrderCommand command, DateTime now, CancellationToken ct)
        {
            UserEntity createdByUser = await GetUser(command.CreatedByUserId, ct);
            UserNicknameEntity userNickname = await userNicknameService.GetOrCreate(command.Nickname, ct);

            DateOnly today = DateOnly.FromDateTime(now);
            ComposerStreamEntity? nearestStream = await uow.ComposerStreamStore.FindNearest(today, ct)
                ?? throw new ReviewOrderException("Нет доступного стрима.");

            ReviewOrderEntity order = uow.ReviewOrderStore.Create(
                now,
                appSettingsService.Settings.ReviewOrderNominalAmount,
                payableAmount: 0,
                command.TrackUrl,
                command.UserComment,
                ReviewOrderType.OutOfQueue,
                nearestStream,
                userNickname,
                createdByUser);

            await uow.SaveChanges(ct);

            orderQueueEventChannel.Write(new ReviewOrderChangedEvent(order, OrderQueueUpdateType.OrderCreated, ReviewOrderStatus.Unspecified));

            return order;
        }

        public async Task<ReviewOrderEntity> CreateDonation(CreateDonationOrderCommand command, DateTime now, CancellationToken ct)
        {
            UserEntity createdByUser = await GetUser(command.CreatedByUserId, ct);
            UserNicknameEntity userNickname = await userNicknameService.GetOrCreate(command.Nickname, ct);
            ComposerStreamEntity nearestStream = await FindNearestStream(userNickname, now, ct)
                ?? throw new ReviewOrderException("Нет доступного стрима.");

            TransactionEntity topUp = uow.TransactionStore.CreateAccountTopUp(
                now,
                command.TopUpProvider,
                command.PaymentAmount,
                userNickname.Account,
                createdByUser);

            ReviewOrderEntity order = uow.ReviewOrderStore.Create(
                now,
                appSettingsService.Settings.ReviewOrderNominalAmount,
                appSettingsService.Settings.ReviewOrderNominalAmount,
                command.TrackUrl,
                command.UserComment,
                ReviewOrderType.Donation,
                nearestStream,
                userNickname,
                createdByUser);

            TransactionEntity payment = uow.TransactionStore.CreatePayment(
                now,
                command.PaymentAmount,
                userNickname.Account,
                order);

            await uow.SaveChanges(ct);

            orderQueueEventChannel.Write(new ReviewOrderChangedEvent(order, OrderQueueUpdateType.OrderCreated, ReviewOrderStatus.Unspecified));

            return order;
        }

        public async Task<ReviewOrderEntity> CreateFree(CreateFreeOrderCommand command, DateTime now, CancellationToken ct)
        {
            UserEntity createdByUser = await GetUser(command.CreatedByUserId, ct);
            UserNicknameEntity userNickname = await userNicknameService.GetOrCreate(command.Nickname, ct);
            ComposerStreamEntity nearestStream = await FindNearestStream(userNickname, now, ct)
                ?? throw new ReviewOrderException("Нет доступного стрима.");

            ReviewOrderEntity order = uow.ReviewOrderStore.Create(
                now,
                appSettingsService.Settings.ReviewOrderNominalAmount,
                payableAmount: 0,
                command.TrackUrl,
                command.UserComment,
                ReviewOrderType.Free,
                nearestStream,
                userNickname,
                createdByUser);

            await uow.SaveChanges(ct);

            orderQueueEventChannel.Write(new ReviewOrderChangedEvent(order, OrderQueueUpdateType.OrderCreated, ReviewOrderStatus.Unspecified));

            return order;
        }

        public async Task<ReviewOrderEntity> CreateCharity(CreateCharityOrderCommand command, DateTime now, CancellationToken ct)
        {
            UserEntity createdByUser = await GetUser(command.CreatedByUserId, ct);
            ComposerStreamEntity? liveStream = await uow.ComposerStreamStore.FindLive(ct);
            if (liveStream is null || liveStream.Type != ComposerStreamType.Charity)
            {
                throw new ReviewOrderException("Не запущен благотворительный стрим.");
            }

            UserNicknameEntity userNickname = await userNicknameService.GetOrCreate(command.Nickname, ct);

            ReviewOrderEntity order = uow.ReviewOrderStore.Create(
                now,
                appSettingsService.Settings.ReviewOrderNominalAmount,
                payableAmount: 0,
                command.TrackUrl,
                command.UserComment,
                ReviewOrderType.Charity,
                liveStream,
                userNickname,
                createdByUser);

            await uow.SaveChanges(ct);

            orderQueueEventChannel.Write(new ReviewOrderChangedEvent(order, OrderQueueUpdateType.OrderCreated, ReviewOrderStatus.Unspecified));

            return order;
        }

        public async Task<TransactionEntity> MoveUp(MoveUpCommand command, DateTime now, CancellationToken ct)
        {
            UserEntity createdByUser = await GetUser(command.CreatedByUserId, ct);
            ReviewOrderEntity order = await GetOrder(command.ReviewOrderId, ct);
            ReviewOrderStatus previousStatus = order.Status;

            if (order.Status is not (ReviewOrderStatus.Preorder or ReviewOrderStatus.Pending))
            {
                throw new ReviewOrderException("Невозможно поднять заказ", order);
            }

            UserNicknameEntity userNickname = await userNicknameService.GetOrCreate(command.Nickname, ct);

            TransactionEntity topUp = uow.TransactionStore.CreateAccountTopUp(
                now,
                command.TopUpProvider,
                command.PaymentAmount,
                userNickname.Account,
                createdByUser);

            TransactionEntity payment = uow.TransactionStore.CreatePayment(
                now,
                command.PaymentAmount,
                userNickname.Account,
                order);

            await uow.SaveChanges(ct);

            orderQueueEventChannel.Write(new ReviewOrderChangedEvent(order, OrderQueueUpdateType.OrderMovedUp, previousStatus));

            return payment;
        }

        public async Task<ReviewOrderEntity> AddTrackUrl(AddTrackUrlCommand command, CancellationToken ct)
        {
            ReviewOrderEntity order = await GetOrder(command.ReviewOrderId, ct);
            ReviewOrderStatus previousStatus = order.Status;

            if (order.Status is not (ReviewOrderStatus.Preorder or ReviewOrderStatus.Pending or ReviewOrderStatus.InProgress))
            {
                throw new ReviewOrderException("Невозможно добавить/изменить ссылку на трек", order);
            }

            order.TrackUrl = command.TrackUrl;

            if (order.Status == ReviewOrderStatus.Preorder)
            {
                order.Status = ReviewOrderStatus.Pending;
            }

            await uow.SaveChanges(ct);

            orderQueueEventChannel.Write(new ReviewOrderChangedEvent(order, OrderQueueUpdateType.TrackUrlAdded, previousStatus));

            return order;
        }

        public async Task<ReviewOrderEntity> TakeInProgress(long reviewOrderId, DateTime now, CancellationToken ct)
        {
            ReviewOrderEntity order = await GetOrder(reviewOrderId, ct);
            ReviewOrderStatus previousStatus = order.Status;

            if (order.Status == ReviewOrderStatus.InProgress)
            {
                return order;
            }

            if (order.IsFrozen || order.Status != ReviewOrderStatus.Pending)
            {
                throw new ReviewOrderException("Невозможно взять в работу заказ", order);
            }

            ComposerStreamEntity liveStream = await uow.ComposerStreamStore.FindLive(ct)
                ?? throw new ReviewOrderException("Невозможно взять в работу заказ вне активного стрима", order);

            ReviewOrderEntity? inProgress = await uow.ReviewOrderQueries.FindInProgress(ct);
            if (inProgress is not null && inProgress.Id != reviewOrderId)
            {
                throw new ReviewOrderException($"Невозможно взять в работу заказ, пока заказ Id: {inProgress.Id} находится в работе", order);
            }

            OrderQueuePosition position = await orderQueueService.GetCurrentQueuePosition(order);

            order.CategoryType = position.Category.Type;
            order.ProcessingStream = liveStream;
            order.Status = ReviewOrderStatus.InProgress;
            order.InProgressAt = now;

            await uow.SaveChanges(ct);

            orderQueueEventChannel.Write(new ReviewOrderChangedEvent(order, OrderQueueUpdateType.OrderTaken, previousStatus));

            return order;
        }

        public async Task<ReviewOrderEntity> Complete(CompleteCommand command, DateTime now, CancellationToken ct)
        {
            ReviewOrderEntity order = await GetOrder(command.ReviewOrderId, ct);
            ReviewOrderStatus previousStatus = order.Status;

            if (order.Status == ReviewOrderStatus.Completed)
            {
                return order;
            }

            if (order.Status != ReviewOrderStatus.InProgress)
            {
                throw new ReviewOrderException("Невозможно выполнить заказ", order);
            }

            UserEntity createdByUser = await GetUser(command.CreatedByUserId, ct);
            order.Review = uow.ReviewStore.Create(order, command.Rating, now, createdByUser);
            order.CompletedAt = now;
            order.Status = ReviewOrderStatus.Completed;

            await uow.SaveChanges(ct);

            orderQueueEventChannel.Write(new ReviewOrderChangedEvent(order, OrderQueueUpdateType.OrderCompleted, previousStatus));

            return order;
        }

        public async Task<ReviewOrderEntity> Freeze(long reviewOrderId, CancellationToken ct)
        {
            ReviewOrderEntity order = await GetOrder(reviewOrderId, ct);
            ReviewOrderStatus previousStatus = order.Status;

            if (order.IsFrozen)
            {
                return order;
            }

            if (order.Status is not (ReviewOrderStatus.Preorder or ReviewOrderStatus.Pending))
            {
                throw new ReviewOrderException("Невозможно заморозить заказ", order);
            }

            order.IsFrozen = true;

            await uow.SaveChanges(ct);

            orderQueueEventChannel.Write(new ReviewOrderChangedEvent(order, OrderQueueUpdateType.OrderFrozen, previousStatus));

            return order;
        }

        public async Task<ReviewOrderEntity> Unfreeze(long reviewOrderId, CancellationToken ct)
        {
            ReviewOrderEntity order = await GetOrder(reviewOrderId, ct);
            ReviewOrderStatus previousStatus = order.Status;

            if (!order.IsFrozen)
            {
                return order;
            }

            if (order.Status is not (ReviewOrderStatus.Preorder or ReviewOrderStatus.Pending))
            {
                throw new ReviewOrderException("Невозможно разморозить заказ", order);
            }

            order.IsFrozen = false;

            await uow.SaveChanges(ct);

            orderQueueEventChannel.Write(new ReviewOrderChangedEvent(order, OrderQueueUpdateType.OrderUnfrozen, previousStatus));

            return order;
        }

        public async Task<ReviewOrderEntity> Cancel(long reviewOrderId, CancellationToken ct)
        {
            ReviewOrderEntity order = await GetOrder(reviewOrderId, ct);
            ReviewOrderStatus previousStatus = order.Status;

            if (order.Status == ReviewOrderStatus.Canceled)
            {
                return order;
            }

            if (order.Status is not (ReviewOrderStatus.Preorder or ReviewOrderStatus.Pending or ReviewOrderStatus.InProgress))
            {
                throw new ReviewOrderException("Невозможно отменить заказ", order);
            }

            order.CategoryType = OrderCategoryType.Unspecified;
            order.ProcessingStream = null;
            order.Status = ReviewOrderStatus.Canceled;
            order.InProgressAt = null;

            await uow.SaveChanges(ct);

            orderQueueEventChannel.Write(new ReviewOrderChangedEvent(order, OrderQueueUpdateType.OrderCanceled, previousStatus));

            return order;
        }

        private async Task<ReviewOrderEntity> GetOrder(long orderId, CancellationToken ct)
        {
            return await uow.ReviewOrderStore.FindById(orderId, ct)
                ?? throw new ReviewOrderException("Заказ не найден.");
        }

        private async Task<UserEntity> GetUser(Guid userId, CancellationToken ct)
        {
            return await userManager.Users.FirstOrDefaultAsync(x => x.Id == userId, ct)
                ?? throw new ReviewOrderException("Пользователь не найден.");
        }

        private async Task<ComposerStreamEntity?> FindNearestStream(UserNicknameEntity userNickname, DateTime now, CancellationToken ct)
        {
            DateOnly today = DateOnly.FromDateTime(now);

            if (await uow.UserNicknameQueries.HasOrders(userNickname, ct))
            {
                return await uow.ComposerStreamStore.FindNearest(today, ComposerStreamType.Donation, ct);
            }
            else
            {
                return await uow.ComposerStreamStore.FindNearest(today, ct);
            }
        }
    }
}