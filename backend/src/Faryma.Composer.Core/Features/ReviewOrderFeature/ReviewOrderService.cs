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
        public async Task<ReviewOrder> Create(CreateCommand command)
        {
            UserNickname userNickname = await userNicknameService.GetOrCreate(command.Nickname);
            ComposerStream stream = await composerStreamService.GetOrCreateForOrder(userNickname, command.OrderType);
            int nominalAmount = appSettingsService.Settings.ReviewOrderNominalAmount;

            ReviewOrder? order = null;
            switch (command.OrderType)
            {
                case ReviewOrderType.Donation:

                    Transaction deposit = uow.TransactionRepository.CreateDeposit(userNickname.Account, command.PaymentAmount!.Value);
                    Transaction payment = uow.TransactionRepository.CreatePayment(userNickname.Account, command.PaymentAmount!.Value);

                    order = uow.ReviewOrderRepository.CreateDonation(
                        stream,
                        payment,
                        nominalAmount,
                        command.TrackUrl,
                        command.UserComment);

                    break;
                case ReviewOrderType.OutOfQueue or ReviewOrderType.Charity:

                    order = uow.ReviewOrderRepository.CreateFree(
                        stream,
                        userNickname,
                        nominalAmount: 0,
                        command.OrderType,
                        command.TrackUrl,
                        command.UserComment);

                    break;
                case ReviewOrderType.Free:

                    order = uow.ReviewOrderRepository.CreateFree(
                        stream,
                        userNickname,
                        nominalAmount,
                        command.OrderType,
                        command.TrackUrl,
                        command.UserComment);

                    break;

                default:
                    throw new ReviewOrderException($"Тип заказа '{command.OrderType}' не поддерживается");
            }

            await uow.SaveChangesAsync();

            await orderQueueService.AddOrder(order);

            return order;
        }

        public async Task<Transaction> Up(UpCommand command)
        {
            ReviewOrder order = await uow.ReviewOrderRepository.Get(command.ReviewOrderId);

            if (order.Status is not (ReviewOrderStatus.Preorder or ReviewOrderStatus.Pending))
            {
                throw new ReviewOrderException($"Невозможно поднять заказ в статусе '{order.Status}'");
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
                throw new ReviewOrderException($"Невозможно добавить ссылку на трек в статусе '{order.Status}'");
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

        public async Task<ReviewOrder> TakeInProgress(TakeInProgressCommand command)
        {
            ReviewOrder order = await uow.ReviewOrderRepository.Get(command.ReviewOrderId);
            if (order.Status == ReviewOrderStatus.InProgress)
            {
                return order;
            }

            if (order.IsFrozen)
            {
                throw new ReviewOrderException("Невозможно взять в работу замороженный заказ");
            }

            if (order.Status != ReviewOrderStatus.Pending)
            {
                throw new ReviewOrderException($"Невозможно взять в работу заказ в статусе '{order.Status}'");
            }

            ReviewOrder? inProgress = await uow.ReviewOrderRepository.FindAnotherOrderInProgress(command.ReviewOrderId);
            if (inProgress is not null)
            {
                throw new ReviewOrderException($"Невозможно взять в работу заказ Id: {command.ReviewOrderId}, пока заказ Id: {inProgress.Id} находится в работе");
            }

            ComposerStream liveStream = await uow.ComposerStreamRepository.FindLive()
                ?? throw new ReviewOrderException("Невозможно взять в работу заказ вне активного стрима");

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
                throw new ReviewOrderException($"Невозможно выполнить заказ в статусе '{order.Status}'");
            }

            DateTime now = DateTime.UtcNow;

            order.Review = uow.ReviewRepository.Create(order, command.Rating, now);
            order.CompletedAt = now;
            order.Status = ReviewOrderStatus.Completed;

            await uow.SaveChangesAsync();

            await orderQueueService.UpdateOrder(order, OrderQueueUpdateType.Complete);

            return order;
        }

        public async Task<ReviewOrder> Freeze(FreezeCommand command)
        {
            ReviewOrder order = await uow.ReviewOrderRepository.Get(command.ReviewOrderId);
            if (order.IsFrozen)
            {
                return order;
            }

            if (order.Status is not (ReviewOrderStatus.Preorder or ReviewOrderStatus.Pending))
            {
                throw new ReviewOrderException($"Невозможно заморозить заказ в статусе '{order.Status}'");
            }

            order.IsFrozen = true;

            await uow.SaveChangesAsync();

            await orderQueueService.UpdateOrder(order, OrderQueueUpdateType.Freeze);

            return order;
        }

        public async Task<ReviewOrder> Unfreeze(UnfreezeCommand command)
        {
            ReviewOrder order = await uow.ReviewOrderRepository.Get(command.ReviewOrderId);
            if (!order.IsFrozen)
            {
                return order;
            }

            if (order.Status is not (ReviewOrderStatus.Preorder or ReviewOrderStatus.Pending))
            {
                throw new ReviewOrderException($"Невозможно разморозить заказ в статусе '{order.Status}'");
            }

            order.IsFrozen = false;

            await uow.SaveChangesAsync();

            await orderQueueService.UpdateOrder(order, OrderQueueUpdateType.Unfreeze);

            return order;
        }

        public async Task<ReviewOrder> Cancel(CancelCommand command)
        {
            ReviewOrder order = await uow.ReviewOrderRepository.Get(command.ReviewOrderId);
            if (order.Status == ReviewOrderStatus.Canceled)
            {
                return order;
            }

            if (order.Status is not (ReviewOrderStatus.Preorder or ReviewOrderStatus.Pending or ReviewOrderStatus.InProgress))
            {
                throw new ReviewOrderException($"Невозможно отменить заказ в статусе '{order.Status}'");
            }

            order.Status = ReviewOrderStatus.Canceled;

            await uow.SaveChangesAsync();

            await orderQueueService.RemoveOrder(order);

            return order;
        }
    }
}