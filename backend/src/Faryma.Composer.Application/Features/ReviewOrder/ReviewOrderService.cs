using Faryma.Composer.Application.Features.AppSettings;
using Faryma.Composer.Application.Features.OrderQueue;
using Faryma.Composer.Application.Features.ReviewOrder.Pricing;
using Faryma.Composer.Application.Features.UserNickname;
using Faryma.Composer.Application.SharedContracts.Features.OrderQueue.Enums;
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
        OrderQueueEventChannel orderQueueEventChannel)
    {
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

            orderQueueEventChannel.Write(order, OrderQueueUpdateType.OrderMovedUp, previousStatus);

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

            orderQueueEventChannel.Write(order, OrderQueueUpdateType.OrderCreated, ReviewOrderStatus.Unspecified);

            return order;
        }

        public async Task<ReviewOrderEntity> GetOrder(long orderId, CancellationToken ct)
        {
            return await uow.ReviewOrderStore.FindById(orderId, ct)
                ?? throw new ReviewOrderException("Заказ не найден");
        }

        public async Task<UserEntity> GetUser(Guid userId, CancellationToken ct)
        {
            return await userManager.Users.FirstOrDefaultAsync(x => x.Id == userId, ct)
                ?? throw new ReviewOrderException("Пользователь не найден");
        }

        public async Task<ComposerStreamEntity?> FindNearestStream(UserNicknameEntity userNickname, CancellationToken ct)
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

        public long GetTrackRequiredAmount(int? trackDurationSeconds)
        {
            AppSettingsEntity settings = appSettingsService.Settings;
            long result = settings.ReviewOrderNominalPrice;

            if (trackDurationSeconds > settings.IncludedTrackDurationSeconds)
            {
                int extraTrackSeconds = trackDurationSeconds.Value - settings.IncludedTrackDurationSeconds;
                result += extraTrackSeconds * settings.ReviewOrderExtraTrackSecondPrice;
            }

            return result;
        }

        public void CreateAccountTopUpAndPayment(
            AccountTopUpProvider topUpProvider,
            long paymentAmount,
            UserEntity createdByUser,
            UserNicknameEntity userNickname,
            ReviewOrderEntity order)
        {
            uow.TransactionStore.CreateAccountTopUp(
                topUpProvider,
                paymentAmount,
                userNickname.Account,
                createdByUser);

            uow.TransactionStore.CreatePayment(
                paymentAmount,
                userNickname.Account,
                order);
        }

        public void CreateAndRedeemAdminCoverage(
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
                order);
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

        private void SynchronizePayableAmount(ReviewOrderEntity order)
        {
            order.PayableAmount = reviewOrderPricingService
                .CalculateRequiredPriceComponents(
                    order.Type,
                    order.Price,
                    order.TrackDurationSeconds)
                .Sum(x => x.Amount);
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
    }
}
