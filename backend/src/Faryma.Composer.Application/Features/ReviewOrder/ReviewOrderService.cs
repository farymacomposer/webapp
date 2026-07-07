using Faryma.Composer.Application.SharedContracts.Features.OrderQueue.Enums;
using Faryma.Composer.Application.SharedContracts.Features.OrderQueue.Events;
using Faryma.Composer.Application.SharedContracts.Features.OrderQueue.Models;
using Faryma.Composer.Application.Features.AppSettings;
using Faryma.Composer.Application.Features.OrderQueue;
using Faryma.Composer.Application.Features.ReviewOrder.Commands;
using Faryma.Composer.Application.Features.ReviewOrder.Models;
using Faryma.Composer.Application.Features.ReviewOrder.Pricing;
using Faryma.Composer.Application.Features.UserNickname;
using Faryma.Composer.Domain.Entities;
using Faryma.Composer.Domain.Entities.TransactionSources;
using Faryma.Composer.Domain.Enums;
using Faryma.Composer.Domain.Exceptions;
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
        ReviewOrderPricingService reviewOrderPricingService,
        OrderQueueService orderQueueService,
        OrderQueueEventChannel orderQueueEventChannel,
        DateTimeService dateTimeService)
    {
        public async Task<ReviewOrderEntity> CreateOutOfQueue(CreateOutOfQueueOrderCommand command, CancellationToken ct = default)
        {
            UserEntity createdByUser = await GetUser(command.CreatedByUserId, ct);
            UserNicknameEntity userNickname = await userNicknameService.GetOrCreate(command.UserNickname, ct);

            ComposerStreamEntity? nearestStream = await uow.ComposerStreamStore.FindNearest(ct)
                ?? throw new ReviewOrderException("Нет доступного ближайшего стрима");

            ReviewOrderStatus status = command.TrackUrl is null
                ? ReviewOrderStatus.Preorder
                : ReviewOrderStatus.Pending;

            ReviewOrderEntity order = uow.ReviewOrderStore.Create(
                ReviewOrderType.OutOfQueue,
                status,
                command.TrackUrl,
                command.TrackDurationSeconds,
                appSettingsService.Settings.ReviewOrderNominalPrice,
                payableAmount: 0,
                command.UserComment,
                nearestStream,
                userNickname,
                createdByUser);

            CreateAndRedeemAdminCoverage(
                order,
                userNickname,
                createdByUser);

            await uow.SaveChanges(ct);

            orderQueueEventChannel.Write(new ReviewOrderChangedEvent(order, OrderQueueUpdateType.OrderCreated, ReviewOrderStatus.Unspecified));

            return order;
        }

        public async Task<ReviewOrderEntity> CreateDonation(CreateDonationOrderCommand command, CancellationToken ct = default)
        {
            UserEntity createdByUser = await GetUser(command.CreatedByUserId, ct);
            UserNicknameEntity userNickname = await userNicknameService.GetOrCreate(command.UserNickname, ct);

            ComposerStreamEntity nearestStream = await FindNearestStream(userNickname, ct)
                ?? throw new ReviewOrderException("Нет доступного ближайшего стрима");

            const ReviewOrderType orderType = ReviewOrderType.Donation;

            long requiredAmount = CalculateRequiredAmount(orderType, command.TrackDurationSeconds);

            ReviewOrderStatus status = DetermineCheckoutStatus(
                orderType,
                command.TrackUrl,
                command.TrackDurationSeconds,
                command.PaymentAmount);

            ReviewOrderEntity order = uow.ReviewOrderStore.Create(
                orderType,
                status,
                command.TrackUrl,
                command.TrackDurationSeconds,
                appSettingsService.Settings.ReviewOrderNominalPrice,
                payableAmount: 0,
                command.UserComment,
                nearestStream,
                userNickname,
                createdByUser);

            uow.TransactionStore.CreateAccountTopUp(
                command.TopUpProvider,
                command.PaymentAmount,
                userNickname.Account,
                createdByUser);

            uow.TransactionStore.CreatePayment(
                command.PaymentAmount,
                userNickname.Account,
                order);

            await uow.SaveChanges(ct);

            orderQueueEventChannel.Write(new ReviewOrderChangedEvent(order, OrderQueueUpdateType.OrderCreated, ReviewOrderStatus.Unspecified));

            return order;
        }

        public async Task<ReviewOrderEntity> CreateFree(CreateFreeOrderCommand command, CancellationToken ct = default)
        {
            UserEntity createdByUser = await GetUser(command.CreatedByUserId, ct);
            UserNicknameEntity userNickname = await userNicknameService.GetOrCreate(command.UserNickname, ct);
            ComposerStreamEntity nearestStream = await FindNearestStream(userNickname, ct)
                ?? throw new ReviewOrderException("Нет доступного ближайшего стрима");

            const ReviewOrderType orderType = ReviewOrderType.Free;

            long coverageAmount = CalculateRequiredAmount(orderType, command.TrackDurationSeconds);

            ReviewOrderEntity order = uow.ReviewOrderStore.Create(
                appSettingsService.Settings.ReviewOrderNominalPrice,
                coverageAmount,
                command.TrackUrl,
                command.TrackDurationSeconds,
                command.UserComment,
                orderType,
                nearestStream,
                userNickname,
                createdByUser,
                status: DetermineCheckoutStatus(orderType, command.TrackUrl, command.TrackDurationSeconds, coverageAmount));

            CreateAndRedeemAdminCoverage(
                order,
                userNickname,
                UserEntitlementTarget.FreeReviewOrder,
                createdByUser);

            await uow.SaveChanges(ct);

            orderQueueEventChannel.Write(new ReviewOrderChangedEvent(order, OrderQueueUpdateType.OrderCreated, ReviewOrderStatus.Unspecified));

            return order;
        }

        public async Task<ReviewOrderEntity> CreateCharity(CreateCharityOrderCommand command, CancellationToken ct = default)
        {
            UserEntity createdByUser = await GetUser(command.CreatedByUserId, ct);
            ComposerStreamEntity? liveStream = await uow.ComposerStreamStore.FindLive(ct);
            if (liveStream is null || liveStream.Type != ComposerStreamType.Charity)
            {
                throw new ReviewOrderException("Нет запущенного благотворительного стрима");
            }

            UserNicknameEntity userNickname = await userNicknameService.GetOrCreate(command.UserNickname, ct);

            ReviewOrderEntity order = uow.ReviewOrderStore.Create(
                appSettingsService.Settings.ReviewOrderNominalPrice,
                payableAmount: 0,
                command.TrackUrl,
                command.TrackDurationSeconds,
                command.UserComment,
                ReviewOrderType.Charity,
                liveStream,
                userNickname,
                createdByUser);

            await uow.SaveChanges(ct);

            orderQueueEventChannel.Write(new ReviewOrderChangedEvent(order, OrderQueueUpdateType.OrderCreated, ReviewOrderStatus.Unspecified));

            return order;
        }

        public async Task<TransactionEntity> PayOrder(PayOrderCommand command, CancellationToken ct = default)
        {
            if (command.PaymentAmount <= 0)
            {
                throw new ReviewOrderException("Сумма платежа должна быть больше нуля");
            }

            UserEntity createdByUser = await GetUser(command.CreatedByUserId, ct);
            ReviewOrderEntity order = await GetOrder(command.ReviewOrderId, ct);
            ReviewOrderStatus previousStatus = order.Status;

            if (order.Status is not (ReviewOrderStatus.Preorder or ReviewOrderStatus.Pending or ReviewOrderStatus.AwaitingPayment))
            {
                throw new ReviewOrderException("Невозможно оплатить заказ", order);
            }

            if (order.Type is not (ReviewOrderType.Donation or ReviewOrderType.Free))
            {
                throw new ReviewOrderException("Тип заказа не поддерживает денежную оплату", order);
            }

            UserNicknameEntity userNickname = await userNicknameService.GetOrCreate(command.Nickname, ct);

            TransactionEntity topUp = uow.TransactionStore.CreateAccountTopUp(
                command.TopUpProvider,
                command.PaymentAmount,
                userNickname.Account,
                createdByUser);

            TransactionEntity payment = uow.TransactionStore.CreatePayment(
                command.PaymentAmount,
                userNickname.Account,
                order);

            RecalculateCheckoutStatus(order);

            await uow.SaveChanges(ct);

            orderQueueEventChannel.Write(new ReviewOrderChangedEvent(order, OrderQueueUpdateType.OrderMovedUp, previousStatus));

            return payment;
        }

        public async Task<ReviewOrderEntity> AddTrackUrl(AddTrackUrlCommand command, CancellationToken ct = default)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(command.TrackDurationSeconds);

            ReviewOrderEntity order = await GetOrder(command.ReviewOrderId, ct);
            ReviewOrderStatus previousStatus = order.Status;

            if (order.Status is not (ReviewOrderStatus.Preorder or ReviewOrderStatus.Pending or ReviewOrderStatus.AwaitingPayment))
            {
                throw new ReviewOrderException("Невозможно добавить/изменить ссылку на трек", order);
            }

            order.TrackUrl = command.TrackUrl;
            order.TrackDurationSeconds = command.TrackDurationSeconds;

            SynchronizePayableAmount(order);
            RecalculateCheckoutStatus(order);

            await uow.SaveChanges(ct);

            orderQueueEventChannel.Write(new ReviewOrderChangedEvent(order, OrderQueueUpdateType.TrackUrlAdded, previousStatus));

            return order;
        }

        public async Task<ReviewOrderEntity> TakeInProgress(long reviewOrderId, CancellationToken ct = default)
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

            order.QueueCategory = position.Category.QueueCategory;
            order.ProcessingStream = liveStream;
            order.Status = ReviewOrderStatus.InProgress;
            order.InProgressAt = dateTimeService.Now;

            await uow.SaveChanges(ct);

            orderQueueEventChannel.Write(new ReviewOrderChangedEvent(order, OrderQueueUpdateType.OrderTaken, previousStatus));

            return order;
        }

        public async Task<ReviewOrderEntity> Complete(CompleteCommand command, CancellationToken ct = default)
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
            order.Review = uow.ReviewStore.Create(order, command.Rating, createdByUser);
            order.CompletedAt = dateTimeService.Now;
            order.Status = ReviewOrderStatus.Completed;

            await uow.SaveChanges(ct);

            orderQueueEventChannel.Write(new ReviewOrderChangedEvent(order, OrderQueueUpdateType.OrderCompleted, previousStatus));

            return order;
        }

        public async Task<ReviewOrderEntity> Freeze(long reviewOrderId, CancellationToken ct = default)
        {
            ReviewOrderEntity order = await GetOrder(reviewOrderId, ct);
            ReviewOrderStatus previousStatus = order.Status;

            if (order.IsFrozen)
            {
                return order;
            }

            if (order.Status is not (ReviewOrderStatus.Preorder or ReviewOrderStatus.Pending or ReviewOrderStatus.AwaitingPayment))
            {
                throw new ReviewOrderException("Невозможно заморозить заказ", order);
            }

            order.IsFrozen = true;

            await uow.SaveChanges(ct);

            orderQueueEventChannel.Write(new ReviewOrderChangedEvent(order, OrderQueueUpdateType.OrderFrozen, previousStatus));

            return order;
        }

        public async Task<ReviewOrderEntity> Unfreeze(long reviewOrderId, CancellationToken ct = default)
        {
            ReviewOrderEntity order = await GetOrder(reviewOrderId, ct);
            ReviewOrderStatus previousStatus = order.Status;

            if (!order.IsFrozen)
            {
                return order;
            }

            if (order.Status is not (ReviewOrderStatus.Preorder or ReviewOrderStatus.Pending or ReviewOrderStatus.AwaitingPayment))
            {
                throw new ReviewOrderException("Невозможно разморозить заказ", order);
            }

            order.IsFrozen = false;

            await uow.SaveChanges(ct);

            orderQueueEventChannel.Write(new ReviewOrderChangedEvent(order, OrderQueueUpdateType.OrderUnfrozen, previousStatus));

            return order;
        }

        public async Task<ReviewOrderEntity> Cancel(CancelCommand command, CancellationToken ct = default)
        {
            ReviewOrderEntity order = await GetOrder(command.ReviewOrderId, ct);
            ReviewOrderStatus previousStatus = order.Status;

            if (order.Status == ReviewOrderStatus.Canceled)
            {
                return order;
            }

            if (order.Status is not (ReviewOrderStatus.Preorder or ReviewOrderStatus.Pending or ReviewOrderStatus.AwaitingPayment or ReviewOrderStatus.InProgress))
            {
                throw new ReviewOrderException("Невозможно отменить заказ", order);
            }

            order.CanceledAt = dateTimeService.Now;
            order.CancelReason = command.CancelReason;
            order.QueueCategory = QueueCategory.Unspecified;
            order.ProcessingStream = null;
            order.Status = ReviewOrderStatus.Canceled;
            order.InProgressAt = null;

            await uow.SaveChanges(ct);

            orderQueueEventChannel.Write(new ReviewOrderChangedEvent(order, OrderQueueUpdateType.OrderCanceled, previousStatus));

            return order;
        }

        public async Task<PayDetailedReviewResult> PayDetailedReview(PayDetailedReviewCommand command, CancellationToken ct = default)
        {
            UserEntity createdByUser = await GetUser(command.CreatedByUserId, ct);
            ReviewOrderEntity order = await GetOrder(command.ReviewOrderId, ct);
            ReviewOrderStatus previousStatus = order.Status;

            if ((command.TopUpProvider is null) == (command.UserEntitlementId is null))
            {
                throw new ReviewOrderException("Укажите либо платеж, либо жетон подробного разбора", order);
            }

            if (order.DetailedReviewPayment is not null)
            {
                throw new ReviewOrderException("Подробный разбор уже оплачен", order);
            }

            long amount = reviewOrderPricingService.CalculateDetailedReviewPaymentAmount();
            if (amount <= 0)
            {
                throw new ReviewOrderException("Стоимость подробного разбора должна быть больше нуля", order);
            }

            UserNicknameEntity userNickname = await userNicknameService.GetOrCreate(command.Nickname, ct);
            ReviewOrderDetailedReviewPaymentEntity source = uow.ReviewOrderStore.CreateDetailedReviewPayment(
                order,
                amount,
                createdByUser);

            TransactionEntity? payment = null;
            UserEntitlementRedemptionEntity? redemption = null;
            if (command.TopUpProvider is AccountTopUpProvider topUpProvider)
            {
                uow.TransactionStore.CreateAccountTopUp(
                    topUpProvider,
                    amount,
                    userNickname.Account,
                    createdByUser);

                payment = uow.TransactionStore.CreatePayment(
                    amount,
                    userNickname.Account,
                    source);
            }
            else
            {
                UserEntitlementEntity entitlement = await uow.UserEntitlementStore.FindById(
                    command.UserEntitlementId!.Value,
                    ct)
                    ?? throw new ReviewOrderException("Жетон не найден", order);

                if (entitlement.UserNicknameId != userNickname.Id)
                {
                    throw new ReviewOrderException("Жетон принадлежит другому псевдониму", order);
                }

                ValidateDetailedReviewEntitlement(entitlement, order);

                redemption = uow.UserEntitlementStore.Redeem(
                    entitlement,
                    UserEntitlementTarget.DetailedReview,
                    amount,
                    createdByUser,
                    detailedReviewPayment: source,
                    comment: "Погашение жетона подробного разбора");
            }

            await uow.SaveChanges(ct);

            orderQueueEventChannel.Write(new ReviewOrderChangedEvent(order, OrderQueueUpdateType.OrderMovedUp, previousStatus));

            return new PayDetailedReviewResult
            {
                ReviewOrder = order,
                PaymentTransaction = payment,
                UserEntitlementRedemption = redemption,
            };
        }

        public async Task<ReviewOrderEntity> CreateWithToken(CreateTokenOrderCommand command, CancellationToken ct = default)
        {
            UserEntity createdByUser = await GetUser(command.CreatedByUserId, ct);
            UserEntitlementEntity entitlement = await uow.UserEntitlementStore.FindById(command.UserEntitlementId, ct)
                ?? throw new ReviewOrderException("Жетон не найден");
            UserNicknameEntity userNickname = await uow.UserNicknameStore.FindByNickname(command.UserNickname, ct)
                ?? throw new ReviewOrderException("Псевдоним не найден");

            if (entitlement.UserNicknameId != userNickname.Id)
            {
                throw new ReviewOrderException("Жетон принадлежит другому псевдониму");
            }

            if (entitlement.UserNickname.UserId is null)
            {
                throw new ReviewOrderException("Жетон не привязан к пользователю");
            }

            if (entitlement.UserNickname.UserId != createdByUser.Id)
            {
                throw new ReviewOrderException("Жетон принадлежит другому пользователю");
            }

            ReviewOrderType orderType = GetOrderType(entitlement.Target);
            UserEntitlementTarget target = GetOrderEntitlementTarget(orderType);
            ValidateServiceToken(entitlement, target);

            ComposerStreamEntity nearestStream = await FindCreationStream(orderType, userNickname, ct)
                ?? throw new ReviewOrderException("Нет доступного стрима");
            long coverageAmount = CalculateRequiredAmount(orderType, command.TrackDurationSeconds);

            ReviewOrderEntity order = uow.ReviewOrderStore.Create(
                appSettingsService.Settings.ReviewOrderNominalPrice,
                coverageAmount,
                command.TrackUrl,
                command.TrackDurationSeconds,
                command.UserComment,
                orderType,
                nearestStream,
                userNickname,
                createdByUser,
                status: DetermineCheckoutStatus(orderType, command.TrackUrl, command.TrackDurationSeconds, coverageAmount));

            uow.UserEntitlementStore.Redeem(
                entitlement,
                target,
                coverageAmount,
                createdByUser,
                reviewOrder: order,
                comment: "Погашение жетона при создании заказа");

            await uow.SaveChanges(ct);

            orderQueueEventChannel.Write(new ReviewOrderChangedEvent(order, OrderQueueUpdateType.OrderCreated, ReviewOrderStatus.Unspecified));

            return order;
        }

        private static ReviewOrderType GetOrderType(UserEntitlementTarget target)
        {
            return target switch
            {
                UserEntitlementTarget.FreeReviewOrder => ReviewOrderType.Free,
                UserEntitlementTarget.OutOfQueueReviewOrder => ReviewOrderType.OutOfQueue,
                UserEntitlementTarget.DetailedReview => throw new ReviewOrderException("Жетон подробного разбора нельзя использовать для создания заказа"),
                _ => throw new ReviewOrderException("Жетон не применим к созданию заказа")
            };
        }

        private static UserEntitlementTarget GetOrderEntitlementTarget(ReviewOrderType orderType)
        {
            return orderType switch
            {
                ReviewOrderType.Free => UserEntitlementTarget.FreeReviewOrder,
                ReviewOrderType.OutOfQueue => UserEntitlementTarget.OutOfQueueReviewOrder,
                _ => throw new ReviewOrderException("Тип заказа не поддерживает жетон")
            };
        }

        private async Task<ReviewOrderEntity> GetOrder(long orderId, CancellationToken ct)
        {
            return await uow.ReviewOrderStore.FindById(orderId, ct)
                ?? throw new ReviewOrderException("Заказ не найден");
        }

        private async Task<UserEntity> GetUser(Guid userId, CancellationToken ct)
        {
            return await userManager.Users.FirstOrDefaultAsync(x => x.Id == userId, ct)
                ?? throw new ReviewOrderException("Пользователь не найден");
        }

        private long CalculateRequiredAmount(ReviewOrderType orderType, int? trackDurationSeconds)
        {
            return reviewOrderPricingService
                .CalculateRequiredPriceComponents(
                    orderType,
                    appSettingsService.Settings.ReviewOrderNominalPrice,
                    trackDurationSeconds)
                .Sum(x => x.Amount);
        }

        private ReviewOrderStatus DetermineCheckoutStatus(
            ReviewOrderType orderType,
            string? trackUrl,
            int? trackDurationSeconds,
            long coveredAmount)
        {
            if (trackUrl is null)
            {
                return ReviewOrderStatus.Preorder;
            }

            long requiredAmount = CalculateRequiredAmount(orderType, trackDurationSeconds);

            return coveredAmount >= requiredAmount
                ? ReviewOrderStatus.Pending
                : ReviewOrderStatus.AwaitingPayment;
        }

        private void RecalculateCheckoutStatus(ReviewOrderEntity order)
        {
            if (order.Status is not (ReviewOrderStatus.Preorder or ReviewOrderStatus.Pending or ReviewOrderStatus.AwaitingPayment))
            {
                return;
            }

            ReviewOrderPricing pricing = reviewOrderPricingService.Calculate(order);
            if (!pricing.IsRequiredCovered)
            {
                order.Status = order.TrackUrl is null
                    ? ReviewOrderStatus.Preorder
                    : ReviewOrderStatus.AwaitingPayment;
                return;
            }

            order.Status = order.TrackUrl is null
                ? ReviewOrderStatus.Preorder
                : ReviewOrderStatus.Pending;
        }

        private void SynchronizePayableAmount(ReviewOrderEntity order)
        {
            order.PayableAmount = reviewOrderPricingService
                .CalculateRequiredPriceComponents(
                    order.Type,
                    order.Price,
                    order.TrackDurationSeconds)
                .Sum(x => x.Amount);
        }

        private void CreateAndRedeemAdminCoverage(
            ReviewOrderEntity order,
            UserNicknameEntity userNickname,
            UserEntity createdByUser)
        {
            UserEntitlementTarget target = order.Type switch
            {
                ReviewOrderType.OutOfQueue => UserEntitlementTarget.OutOfQueueReviewOrder,
                ReviewOrderType.Free => UserEntitlementTarget.FreeReviewOrder,
                _ => throw new ReviewOrderException("Тип заказа не поддерживает жетон", order)
            };

            UserEntitlementEntity entitlement = uow.UserEntitlementStore.Create(
                target,
                userNickname,
                createdByUser);

            uow.UserEntitlementStore.Redeem(
                entitlement,
                target,
                createdByUser,
                reviewOrder: order);
        }

        private void ValidateDetailedReviewEntitlement(UserEntitlementEntity entitlement, ReviewOrderEntity order) =>
            ValidateServiceToken(entitlement, UserEntitlementTarget.DetailedReview, order);

        private void ValidateServiceToken(
            UserEntitlementEntity entitlement,
            UserEntitlementTarget target,
            ReviewOrderEntity? order = null)
        {
            if (entitlement.Target != target)
            {
                throw new ReviewOrderException("Жетон не применим к выбранной услуге", order);
            }

            if (entitlement.RedeemedAt is not null || entitlement.Redemption is not null)
            {
                throw new ReviewOrderException("Жетон уже погашен", order);
            }

            if (entitlement.CanceledAt is not null)
            {
                throw new ReviewOrderException("Жетон отменен", order);
            }
        }

        private async Task<ComposerStreamEntity?> FindCreationStream(
            ReviewOrderType orderType,
            UserNicknameEntity userNickname,
            CancellationToken ct)
        {
            return orderType switch
            {
                ReviewOrderType.Free => await FindNearestStream(userNickname, ct),
                ReviewOrderType.OutOfQueue => await uow.ComposerStreamStore.FindNearest(ct),
                _ => throw new ReviewOrderException("Тип заказа не поддерживает создание по жетону")
            };
        }

        private async Task<ComposerStreamEntity?> FindNearestStream(UserNicknameEntity userNickname, CancellationToken ct)
        {
            if (await uow.UserNicknameQueries.HasOrders(userNickname, ct))
            {
                return await uow.ComposerStreamStore.FindNearest(ComposerStreamType.Donation, ct);
            }
            else
            {
                return await uow.ComposerStreamStore.FindNearest(ct);
            }
        }
    }
}
