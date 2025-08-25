using Faryma.Composer.Core.Features.AppSettings;
using Faryma.Composer.Core.Features.ComposerStreamFeature;
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
        ComposerStreamService composerStreamService,
        OrderQueueService orderQueueService)
    {
        public async Task<ReviewOrder> CreateOutOfQueue(CreateOutOfQueueOrderCommand command)
        {
            UserNickname userNickname = await userNicknameService.GetOrCreate(command.Nickname);
            ComposerStream stream = await composerStreamService.GetOrCreateForOutOfQueueOrder();

            ReviewOrder order = uow.ReviewOrderRepository.CreateFree(
                stream,
                userNickname,
                appSettingsService.Settings.ReviewOrderNominalAmount,
                command.TrackUrl,
                command.UserComment,
                ReviewOrderType.OutOfQueue);

            await uow.SaveChangesAsync();

            await orderQueueService.AddOrder(order);

            return order;
        }

        public async Task<ReviewOrder> CreateDonation(CreateDonationOrderCommand command)
        {
            UserNickname userNickname = await userNicknameService.GetOrCreate(command.Nickname);
            ComposerStream stream = await composerStreamService.GetOrCreateForOrder(userNickname);

            Transaction deposit = uow.TransactionRepository.CreateDeposit(userNickname.Account, command.PaymentAmount);
            Transaction payment = uow.TransactionRepository.CreatePayment(userNickname.Account, command.PaymentAmount);

            ReviewOrder order = uow.ReviewOrderRepository.CreateDonation(
                stream,
                payment,
                appSettingsService.Settings.ReviewOrderNominalAmount,
                command.TrackUrl,
                command.UserComment);

            await uow.SaveChangesAsync();

            await orderQueueService.AddOrder(order);

            return order;
        }

        public async Task<ReviewOrder> CreateFree(CreateFreeOrderCommand command)
        {
            UserNickname userNickname = await userNicknameService.GetOrCreate(command.Nickname);
            ComposerStream stream = await composerStreamService.GetOrCreateForOrder(userNickname);

            ReviewOrder order = uow.ReviewOrderRepository.CreateFree(
                stream,
                userNickname,
                appSettingsService.Settings.ReviewOrderNominalAmount,
                command.TrackUrl,
                command.UserComment,
                ReviewOrderType.Free);

            await uow.SaveChangesAsync();

            await orderQueueService.AddOrder(order);

            return order;
        }

        public async Task<ReviewOrder> CreateCharity(CreateCharityOrderCommand command)
        {
            ComposerStream? stream = await uow.ComposerStreamRepository.FindLive();
            if (stream is null || stream.Type == ComposerStreamType.Charity)
            {
                throw new ReviewOrderException("Благотворительный заказ можно создать только на благотворительном стриме");
            }

            UserNickname userNickname = await userNicknameService.GetOrCreate(command.Nickname);

            ReviewOrder order = uow.ReviewOrderRepository.CreateFree(
                stream,
                userNickname,
                appSettingsService.Settings.ReviewOrderNominalAmount,
                command.TrackUrl,
                command.UserComment,
                ReviewOrderType.Charity);

            await uow.SaveChangesAsync();

            await orderQueueService.AddOrder(order);

            return order;
        }

        public async Task<Transaction> Up(UpCommand command)
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

            await orderQueueService.UpdateOrder(order, OrderQueueUpdateType.Up);

            return payment;
        }

        public async Task<ReviewOrder> AddTrackUrl(AddTrackUrlCommand command)
        {
            ReviewOrder order = await uow.ReviewOrderRepository.Get(command.ReviewOrderId);

            if (order.Status is not (ReviewOrderStatus.Preorder or ReviewOrderStatus.Pending or ReviewOrderStatus.InProgress))
            {
                throw new ReviewOrderException("Невозможно добавить ссылку на трек", order);
            }

            order.TrackUrl = command.TrackUrl;

            if (order.Status == ReviewOrderStatus.Preorder)
            {
                order.Status = ReviewOrderStatus.Pending;
            }

            await uow.SaveChangesAsync();

            await orderQueueService.UpdateOrder(order, OrderQueueUpdateType.AddTrackUrl);

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

            ReviewOrder? inProgress = await uow.ReviewOrderRepository.FindAnotherOrderInProgress(reviewOrderId);
            if (inProgress is not null)
            {
                throw new ReviewOrderException($"Невозможно взять в работу заказ, пока заказ Id: {inProgress.Id} находится в работе", order);
            }

            ComposerStream liveStream = await uow.ComposerStreamRepository.FindLive()
                ?? throw new ReviewOrderException("Невозможно взять в работу заказ вне активного стрима", order);

            OrderQueuePosition position = await orderQueueService.GetCurrentQueuePosition(order);

            order.CategoryType = position.Category.Type;
            order.ProcessingStream = liveStream;
            order.Status = ReviewOrderStatus.InProgress;
            order.InProgressAt = DateTime.UtcNow;

            await uow.SaveChangesAsync();

            await orderQueueService.UpdateOrder(order, OrderQueueUpdateType.TakeInProgress);

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

            await orderQueueService.UpdateOrder(order, OrderQueueUpdateType.Complete);

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

            await orderQueueService.UpdateOrder(order, OrderQueueUpdateType.Freeze);

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

            await orderQueueService.UpdateOrder(order, OrderQueueUpdateType.Unfreeze);

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

            order.Status = ReviewOrderStatus.Canceled;

            await uow.SaveChangesAsync();

            await orderQueueService.RemoveOrder(order);

            return order;
        }
    }
}