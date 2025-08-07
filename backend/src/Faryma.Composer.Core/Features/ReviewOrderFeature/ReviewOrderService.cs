using Faryma.Composer.Core.Features.AppSettings;
using Faryma.Composer.Core.Features.ComposerStreamFeature;
using Faryma.Composer.Core.Features.OrderQueueFeature;
using Faryma.Composer.Core.Features.ReviewOrderFeature.Commands;
using Faryma.Composer.Core.Features.UserNicknameFeature;
using Faryma.Composer.Infrastructure;
using Faryma.Composer.Infrastructure.Entities;
using Faryma.Composer.Infrastructure.Enums;

namespace Faryma.Composer.Core.Features.ReviewOrderFeature
{
    public sealed class ReviewOrderService(
        UnitOfWork ofw,
        UserNicknameService userNicknameService,
        AppSettingsService appSettingsService,
        ComposerStreamService composerStreamService,
        OrderQueueService orderQueueService)
    {
        public async Task<ReviewOrder> Create(CreateCommand command)
        {
            UserNickname userNickname = await userNicknameService.GetOrCreate(command.Nickname);
            ComposerStream stream = await composerStreamService.GetOrCreateForOrder(userNickname, command.OrderType);

            ReviewOrder? result = null;
            switch (command.OrderType)
            {
                case ReviewOrderType.Donation:

                    Transaction deposit = ofw.TransactionRepository.CreateDeposit(userNickname.Account, command.PaymentAmount!.Value);
                    Transaction payment = ofw.TransactionRepository.CreatePayment(userNickname.Account, command.PaymentAmount!.Value);

                    result = ofw.ReviewOrderRepository.CreateDonation(
                        stream,
                        payment,
                        command.OrderType,
                        command.TrackUrl,
                        command.UserComment);

                    break;
                case ReviewOrderType.OutOfQueue or ReviewOrderType.Charity:

                    result = ofw.ReviewOrderRepository.CreateFree(
                        stream,
                        userNickname,
                        nominalAmount: 0,
                        command.OrderType,
                        command.TrackUrl,
                        command.UserComment);

                    break;
                case ReviewOrderType.Free:

                    result = ofw.ReviewOrderRepository.CreateFree(
                        stream,
                        userNickname,
                        appSettingsService.Settings.ReviewOrderNominalAmount,
                        command.OrderType,
                        command.TrackUrl,
                        command.UserComment);

                    break;

                default:
                    throw new ReviewOrderException($"Типа заказа '{command.OrderType}' не поддерживается");
            }

            await ofw.SaveChangesAsync();

            await orderQueueService.AddOrder(result);

            return result;
        }

        public async Task<Transaction> Up(UpCommand command)
        {
            ReviewOrder order = await ofw.ReviewOrderRepository.Get(command.ReviewOrderId);

            if (order.Status is not (ReviewOrderStatus.Pending or ReviewOrderStatus.Preorder))
            {
                throw new ReviewOrderException($"Невозможно поднять заказ в статусе '{order.Status}'");
            }

            UserNickname userNickname = await userNicknameService.GetOrCreate(command.Nickname);
            Transaction deposit = ofw.TransactionRepository.CreateDeposit(userNickname.Account, command.PaymentAmount);
            Transaction payment = ofw.TransactionRepository.CreatePayment(userNickname.Account, command.PaymentAmount);
            order.Payments.Add(payment);

            await ofw.SaveChangesAsync();

            await orderQueueService.UpdateOrder(order);

            return payment;
        }

        public async Task Freeze(FreezeCommand command)
        {
            ReviewOrder order = await ofw.ReviewOrderRepository.Get(command.ReviewOrderId);

            if (order.Status is not (ReviewOrderStatus.Pending or ReviewOrderStatus.Preorder))
            {
                throw new ReviewOrderException($"Невозможно заморозить заказ в статусе '{order.Status}'");
            }

            order.IsFrozen = true;

            await ofw.SaveChangesAsync();

            await orderQueueService.UpdateOrder(order);
        }

        public async Task Unfreeze(UnfreezeCommand command)
        {
            ReviewOrder order = await ofw.ReviewOrderRepository.Get(command.ReviewOrderId);

            if (order.Status is not (ReviewOrderStatus.Pending or ReviewOrderStatus.Preorder))
            {
                throw new ReviewOrderException($"Невозможно разморозить заказ в статусе '{order.Status}'");
            }

            order.IsFrozen = false;

            await ofw.SaveChangesAsync();

            await orderQueueService.UpdateOrder(order);
        }

        public async Task Cancel(CancelCommand command)
        {
            ReviewOrder order = await ofw.ReviewOrderRepository.Get(command.ReviewOrderId);

            if (order.Status is not (ReviewOrderStatus.Pending or ReviewOrderStatus.Preorder or ReviewOrderStatus.InProgress))
            {
                throw new ReviewOrderException($"Невозможно отменить заказ в статусе '{order.Status}'");
            }

            order.Status = ReviewOrderStatus.Canceled;

            await ofw.SaveChangesAsync();

            await orderQueueService.RemoveOrder(order);
        }

        public async Task StartReview(StartReviewCommand command)
        {
            ReviewOrder order = await ofw.ReviewOrderRepository.Get(command.ReviewOrderId);

            if (order.IsFrozen)
            {
                throw new ReviewOrderException("Невозможно взять в работу замороженный заказ");
            }

            if (order.Status != ReviewOrderStatus.Pending)
            {
                throw new ReviewOrderException($"Невозможно взять в работу заказ в статусе '{order.Status}'");
            }

            ReviewOrder? inProgress = await ofw.ReviewOrderRepository.FindAnotherOrderInProgress(command.ReviewOrderId);
            if (inProgress is not null)
            {
                throw new ReviewOrderException($"Невозможно взять в работу заказ Id: {command.ReviewOrderId}, пока заказ Id: {inProgress.Id} находится в работе");
            }

            ComposerStream liveStream = await ofw.ComposerStreamRepository.FindLiveStream()
                ?? throw new ReviewOrderException("Невозможно взять в работу заказ вне активного стрима");

            order.ProcessingStream = liveStream;
            order.Status = ReviewOrderStatus.InProgress;
            order.InProgressAt = DateTime.UtcNow;

            await ofw.SaveChangesAsync();

            await orderQueueService.StartReview(order);
        }

        public async Task<string> AddTrackUrl(AddTrackUrlCommand command)
        {
            ReviewOrder order = await ofw.ReviewOrderRepository.Get(command.ReviewOrderId);

            if (order.Status is not (ReviewOrderStatus.Pending or ReviewOrderStatus.Preorder or ReviewOrderStatus.InProgress))
            {
                throw new ReviewOrderException($"Невозможно добавить ссылку на трек в статусе '{order.Status}'");
            }

            order.TrackUrl = command.TrackUrl;

            if (order.Status is ReviewOrderStatus.Preorder)
            {
                order.Status = ReviewOrderStatus.Pending;
            }

            await ofw.SaveChangesAsync();

            await orderQueueService.UpdateOrder(order);

            return order.TrackUrl;
        }
    }
}