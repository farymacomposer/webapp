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
            AppSettingsEntity appSettings = appSettingsService.Settings;
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
                case ReviewOrderType.OutOfQueue:

                    result = ofw.ReviewOrderRepository.CreateFree(
                        stream,
                        userNickname,
                        command.OrderType,
                        nominalAmount: 0,
                        command.TrackUrl,
                        command.UserComment);

                    break;
                case ReviewOrderType.Free:

                    result = ofw.ReviewOrderRepository.CreateFree(
                        stream,
                        userNickname,
                        command.OrderType,
                        appSettings.ReviewOrderNominalAmount,
                        command.TrackUrl,
                        command.UserComment);

                    break;
            }

            await ofw.SaveChangesAsync();

            await orderQueueService.AddOrder(result!);

            return result!;
        }

        public async Task<Transaction> Up(UpCommand command)
        {
            ReviewOrder order = await ofw.ReviewOrderRepository.Find(command.ReviewOrderId)
                ?? throw new ReviewOrderException($"Заказ разбора трека Id: {command.ReviewOrderId}, не существует");

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
            ReviewOrder order = await ofw.ReviewOrderRepository.Find(command.ReviewOrderId)
                ?? throw new ReviewOrderException($"Заказ разбора трека Id: {command.ReviewOrderId}, не существует");

            if (order.Status is not (ReviewOrderStatus.Pending or ReviewOrderStatus.Preorder))
            {
                throw new ReviewOrderException($"Невозможно поднять заказ в статусе '{order.Status}'");
            }

            order.IsFrozen = true;

            await ofw.SaveChangesAsync();

            await orderQueueService.UpdateOrder(order);
        }

        public async Task Cancel(CancelCommand command)
        {
            ReviewOrder order = await ofw.ReviewOrderRepository.Find(command.ReviewOrderId)
                ?? throw new ReviewOrderException($"Заказ разбора трека Id: {command.ReviewOrderId}, не существует");

            if (order.Status is not (ReviewOrderStatus.Pending or ReviewOrderStatus.Preorder))
            {
                throw new ReviewOrderException($"Невозможно поднять заказ в статусе '{order.Status}'");
            }

            order.Status = ReviewOrderStatus.Canceled;

            await ofw.SaveChangesAsync();

            await orderQueueService.RemoveOrder(order);
        }

        public async Task StartReview(CancelCommand command)
        {
            ReviewOrder order = await ofw.ReviewOrderRepository.Find(command.ReviewOrderId)
                ?? throw new ReviewOrderException($"Заказ разбора трека Id: {command.ReviewOrderId}, не существует");

            if (order.Status != ReviewOrderStatus.Pending)
            {
                throw new ReviewOrderException($"Невозможно поднять заказ в статусе '{order.Status}'");
            }

            order.Status = ReviewOrderStatus.InProgress;
            order.InProgressAt = DateTime.UtcNow;

            await ofw.SaveChangesAsync();

            await orderQueueService.StartReview(order);
        }
    }
}