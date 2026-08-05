using Faryma.Composer.Application.Features.AppSettings;
using Faryma.Composer.Application.Features.OrderQueue;
using Faryma.Composer.Application.Features.ReviewOrder;
using Faryma.Composer.Application.Features.ReviewOrder.CreateDonation;
using Faryma.Composer.Application.Features.ReviewOrder.Pay;
using Faryma.Composer.Application.SharedContracts.Features.OrderQueue.Models;
using Faryma.Composer.Application.Test.Infrastructure;
using Faryma.Composer.Domain.Entities;
using Faryma.Composer.Domain.Entities.TransactionSources;
using Faryma.Composer.Domain.Enums;
using Faryma.Composer.Domain.Exceptions;

namespace Faryma.Composer.Application.Test.ReviewOrder
{
    public sealed class PayReviewOrderTests(PostgreSqlFixture fixture) : TestBase(fixture)
    {
        /// <summary>
        /// Проверяет, что доплата по заказу создает платеж для допустимого статуса.
        /// </summary>
        [Theory]
        [InlineData(ReviewOrderStatus.Preorder)]
        [InlineData(ReviewOrderStatus.Pending)]
        [InlineData(ReviewOrderStatus.AwaitingPayment)]
        public async Task PayOrder_AddsPayment_WhenOrderHasValidStatus(ReviewOrderStatus status)
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("admin");
            ReviewOrderEntity order = await app.Data.CreateReviewOrderAsync(
                createdByUserId: user.Id,
                type: ReviewOrderType.Donation,
                status: status,
                trackUrl: status == ReviewOrderStatus.Preorder ? null : "https://example.com/track");

            TransactionEntity payment = await app.RunScopeAsync(services =>
                services.GetRequiredService<ReviewOrderService>().PayOrder(new PayCommand
                {
                    ReviewOrderId = order.Id,
                    Nickname = "Nick-Pay",
                    PaymentAmount = 500,
                    TopUpProvider = AccountTopUpProvider.Manual,
                    CreatedByUserId = user.Id,
                }));

            List<TransactionEntity> orderTransactions = await app.GetOrderTransactionsAsync(order.Id);
            List<TransactionEntity> accountTransactions = await app.RunScopeAsync(async services =>
            {
                IDbContextFactory<AppDbContext> factory = services.GetRequiredService<IDbContextFactory<AppDbContext>>();
                await using AppDbContext context = await factory.CreateDbContextAsync();
                Guid accountId = orderTransactions[0].UserNicknameAccountId;
                return await context.Transactions
                    .AsNoTracking()
                    .Where(x => x.UserNicknameAccountId == accountId)
                    .OrderBy(x => x.Id)
                    .ToListAsync();
            });

            Assert.Equal(TransactionKind.Payment, payment.Kind);
            Assert.Equal(order.Id, payment.TransactionSourceId);
            Assert.Single(orderTransactions);
            Assert.Equal(500, orderTransactions[0].Debit);
            Assert.Contains(accountTransactions, x => x.Kind == TransactionKind.AccountTopUp && x.Credit == 500);
            Assert.Contains(accountTransactions, x => x.Kind == TransactionKind.Payment && x.Debit == 500);
        }

        /// <summary>
        /// Проверяет, что доплата запрещена для недопустимых статусов заказа.
        /// </summary>
        [Theory]
        [InlineData(ReviewOrderStatus.InProgress)]
        [InlineData(ReviewOrderStatus.Completed)]
        [InlineData(ReviewOrderStatus.Canceled)]
        public async Task PayOrder_Throws_WhenOrderHasInvalidStatus(ReviewOrderStatus status)
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("admin");
            ReviewOrderEntity order = await app.Data.CreateReviewOrderAsync(
                createdByUserId: user.Id,
                status: status,
                inProgressAt: status == ReviewOrderStatus.InProgress ? app.FixedNow : null,
                completedAt: status == ReviewOrderStatus.Completed ? app.FixedNow : null,
                canceledAt: status == ReviewOrderStatus.Canceled ? app.FixedNow : null,
                cancelReason: status == ReviewOrderStatus.Canceled ? "reason" : null);

            await Assert.ThrowsAsync<ReviewOrderException>(() =>
                app.RunScopeAsync(services =>
                    services.GetRequiredService<ReviewOrderService>().PayOrder(new PayCommand
                    {
                        ReviewOrderId = order.Id,
                        Nickname = "Nick-Pay",
                        PaymentAmount = 500,
                        TopUpProvider = AccountTopUpProvider.Manual,
                        CreatedByUserId = user.Id,
                    })));
        }

        /// <summary>
        /// Проверяет, что доплата разрешена только для денежных типов заказов.
        /// </summary>
        [Theory]
        [InlineData(ReviewOrderType.Donation)]
        [InlineData(ReviewOrderType.Free)]
        public async Task PayOrder_AddsPayment_WhenOrderTypeAcceptsPayments(ReviewOrderType orderType)
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("admin");
            ReviewOrderEntity order = await app.Data.CreateReviewOrderAsync(
                createdByUserId: user.Id,
                type: orderType,
                status: ReviewOrderStatus.Pending,
                trackUrl: "https://example.com/track");

            TransactionEntity payment = await app.RunScopeAsync(services =>
                services.GetRequiredService<ReviewOrderService>().PayOrder(new PayCommand
                {
                    ReviewOrderId = order.Id,
                    Nickname = "Nick-PayableType",
                    PaymentAmount = 500,
                    TopUpProvider = AccountTopUpProvider.Manual,
                    CreatedByUserId = user.Id,
                }));

            List<TransactionEntity> orderTransactions = await app.GetOrderTransactionsAsync(order.Id);

            Assert.Equal(TransactionKind.Payment, payment.Kind);
            Assert.Equal(order.Id, payment.TransactionSourceId);
            Assert.Single(orderTransactions);
            Assert.Equal(500, orderTransactions[0].Debit);
        }

        /// <summary>
        /// Проверяет, что доплата запрещена для типов заказов без денежных платежей.
        /// </summary>
        [Theory]
        [InlineData(ReviewOrderType.OutOfQueue)]
        [InlineData(ReviewOrderType.Charity)]
        [InlineData(ReviewOrderType.Custom)]
        public async Task PayOrder_Throws_WhenOrderTypeDoesNotAcceptPayments(ReviewOrderType orderType)
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("admin");
            ReviewOrderEntity order = await app.Data.CreateReviewOrderAsync(
                createdByUserId: user.Id,
                type: orderType,
                status: ReviewOrderStatus.Pending,
                trackUrl: "https://example.com/track");

            await Assert.ThrowsAsync<ReviewOrderException>(() =>
                app.RunScopeAsync(services =>
                    services.GetRequiredService<ReviewOrderService>().PayOrder(new PayCommand
                    {
                        ReviewOrderId = order.Id,
                        Nickname = "Nick-NonPayableType",
                        PaymentAmount = 500,
                        TopUpProvider = AccountTopUpProvider.Manual,
                        CreatedByUserId = user.Id,
                    })));

            List<TransactionEntity> orderTransactions = await app.GetOrderTransactionsAsync(order.Id);

            Assert.Empty(orderTransactions);
        }

        /// <summary>
        /// Проверяет, что заморозка заказа не блокирует оплату.
        /// </summary>
        [Fact]
        public async Task PayOrder_AddsPayment_WhenOrderIsFrozen()
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("admin");
            ReviewOrderEntity order = await app.Data.CreateReviewOrderAsync(
                createdByUserId: user.Id,
                status: ReviewOrderStatus.AwaitingPayment,
                isFrozen: true);

            TransactionEntity payment = await app.RunScopeAsync(services =>
                services.GetRequiredService<ReviewOrderService>().PayOrder(new PayCommand
                {
                    ReviewOrderId = order.Id,
                    Nickname = "Nick-Frozen",
                    PaymentAmount = 500,
                    TopUpProvider = AccountTopUpProvider.Manual,
                    CreatedByUserId = user.Id,
                }));
            ReviewOrderEntity persisted = await app.GetOrderAsync(order.Id);
            List<TransactionEntity> orderTransactions = await app.GetOrderTransactionsAsync(order.Id);

            Assert.Equal(TransactionKind.Payment, payment.Kind);
            Assert.True(persisted.IsFrozen);
            Assert.Single(orderTransactions);
            Assert.Equal(500, orderTransactions[0].Debit);
        }

        /// <summary>
        /// Проверяет, что application-слой не принимает нулевые платежи.
        /// </summary>
        [Fact]
        public async Task PayOrder_Throws_WhenPaymentAmountIsZero()
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("admin");
            ReviewOrderEntity order = await app.Data.CreateReviewOrderAsync(
                createdByUserId: user.Id,
                status: ReviewOrderStatus.AwaitingPayment);

            await Assert.ThrowsAsync<ReviewOrderException>(() =>
                app.RunScopeAsync(services =>
                    services.GetRequiredService<ReviewOrderService>().PayOrder(new PayCommand
                    {
                        ReviewOrderId = order.Id,
                        Nickname = "Nick-Zero",
                        PaymentAmount = 0,
                        TopUpProvider = AccountTopUpProvider.Manual,
                        CreatedByUserId = user.Id,
                    })));
        }

        /// <summary>
        /// Проверяет, что для несуществующего заказа выбрасывается ошибка.
        /// </summary>
        [Fact]
        public async Task PayOrder_Throws_WhenOrderDoesNotExist()
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("admin");

            await Assert.ThrowsAsync<ReviewOrderException>(() =>
                app.RunScopeAsync(services =>
                    services.GetRequiredService<ReviewOrderService>().PayOrder(new PayCommand
                    {
                        ReviewOrderId = long.MaxValue,
                        Nickname = "Nick-Pay",
                        PaymentAmount = 500,
                        TopUpProvider = AccountTopUpProvider.Manual,
                        CreatedByUserId = user.Id,
                    })));
        }

        /// <summary>
        /// Проверяет, что доплата улучшает позицию заказа в очереди.
        /// </summary>
        [Fact]
        public async Task PayOrder_ImprovesOrderPositionInQueue()
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("admin");
            _ = await app.Data.CreateStreamAsync(
                createdByUserId: user.Id,
                eventDate: app.Today,
                type: ComposerStreamType.Donation,
                status: ComposerStreamStatus.Live,
                startedAt: app.FixedNow);

            ReviewOrderEntity strongerOrder = await app.RunScopeAsync(services =>
                services.GetRequiredService<ReviewOrderService>().CreateDonation(new CreateDonationCommand
                {
                    UserNickname = "Nick-Strong",
                    TrackUrl = "https://example.com/strong",
                    TrackDurationSeconds = 60,
                    UserComment = null,
                    PaymentAmount = 1_000,
                    TopUpProvider = AccountTopUpProvider.Manual,
                    CreatedByUserId = user.Id,
                }));
            await app.DrainQueueEventsAsync();

            ReviewOrderEntity candidate = await app.RunScopeAsync(services =>
                services.GetRequiredService<ReviewOrderService>().CreateDonation(new CreateDonationCommand
                {
                    UserNickname = "Nick-Candidate",
                    TrackUrl = "https://example.com/candidate",
                    TrackDurationSeconds = 60,
                    UserComment = null,
                    PaymentAmount = 700,
                    TopUpProvider = AccountTopUpProvider.Manual,
                    CreatedByUserId = user.Id,
                }));
            await app.DrainQueueEventsAsync();

            OrderQueueSnapshot beforeSnapshot = await app.RunScopeAsync(services =>
                services.GetRequiredService<OrderQueueService>().GetQueueSnapshot());
            int beforeIndex = beforeSnapshot.Positions.Single(x => x.Order.Id == candidate.Id).PositionHistory.Current.QueueIndex;

            await app.RunScopeAsync(services =>
                services.GetRequiredService<ReviewOrderService>().PayOrder(new PayCommand
                {
                    ReviewOrderId = candidate.Id,
                    Nickname = "Nick-Candidate",
                    PaymentAmount = 500,
                    TopUpProvider = AccountTopUpProvider.Manual,
                    CreatedByUserId = user.Id,
                }));
            await app.DrainQueueEventsAsync();

            OrderQueueSnapshot afterSnapshot = await app.RunScopeAsync(services =>
                services.GetRequiredService<OrderQueueService>().GetQueueSnapshot());
            int afterIndex = afterSnapshot.Positions.Single(x => x.Order.Id == candidate.Id).PositionHistory.Current.QueueIndex;
            int strongerIndex = afterSnapshot.Positions.Single(x => x.Order.Id == strongerOrder.Id).PositionHistory.Current.QueueIndex;

            Assert.True(afterIndex < beforeIndex);
            Assert.True(afterIndex < strongerIndex);
        }

        /// <summary>
        /// Проверяет, что общий платеж по заказу переводит заказ из ожидания оплаты в готовые.
        /// </summary>
        [Fact]
        public async Task PayOrder_MovesAwaitingPaymentToPending_WhenRequiredAmountBecomesCovered()
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("admin");
            ReviewOrderEntity order = await app.Data.CreateReviewOrderAsync(
                createdByUserId: user.Id,
                nickname: "Nick-Author",
                type: ReviewOrderType.Donation,
                status: ReviewOrderStatus.AwaitingPayment,
                totalPaymentAmount: 600);

            TransactionEntity payment = await app.RunScopeAsync(services =>
                services.GetRequiredService<ReviewOrderService>().PayOrder(new PayCommand
                {
                    ReviewOrderId = order.Id,
                    Nickname = "Nick-Payer",
                    PaymentAmount = 400,
                    TopUpProvider = AccountTopUpProvider.Manual,
                    CreatedByUserId = user.Id,
                }));

            ReviewOrderEntity persisted = await app.GetOrderAsync(order.Id);
            List<TransactionEntity> orderTransactions = await app.GetOrderTransactionsAsync(order.Id);

            Assert.Equal(TransactionKind.Payment, payment.Kind);
            Assert.Equal(ReviewOrderStatus.Pending, persisted.Status);
            Assert.Equal("Nick-Author", persisted.MainNickname);
            Assert.Equal([600, 400], orderTransactions.Select(x => x.Debit).ToArray());
            Assert.Equal(2, orderTransactions.Select(x => x.UserNicknameAccountId).Distinct().Count());
        }

        /// <summary>
        /// Проверяет, что обязательная доплата за длинный трек закрывается обычным платежом заказа.
        /// </summary>
        [Fact]
        public async Task PayOrder_MovesAwaitingPaymentToPending_WhenExtraDurationAmountBecomesCovered()
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("admin");
            ReviewOrderEntity order = await app.Data.CreateReviewOrderAsync(
                createdByUserId: user.Id,
                type: ReviewOrderType.Donation,
                status: ReviewOrderStatus.AwaitingPayment,
                trackDurationSeconds: 420,
                payableAmount: 1_110,
                totalPaymentAmount: 750);

            TransactionEntity payment = await app.RunScopeAsync(services =>
                services.GetRequiredService<ReviewOrderService>().PayOrder(new PayCommand
                {
                    ReviewOrderId = order.Id,
                    Nickname = "Nick-ExtraDuration",
                    PaymentAmount = 360,
                    TopUpProvider = AccountTopUpProvider.Manual,
                    CreatedByUserId = user.Id,
                }));

            ReviewOrderEntity persisted = await app.GetOrderAsync(order.Id);
            List<TransactionEntity> orderTransactions = await app.GetOrderTransactionsAsync(order.Id);

            Assert.Equal(TransactionKind.Payment, payment.Kind);
            Assert.Equal(ReviewOrderStatus.Pending, persisted.Status);
            Assert.Equal(1_110, persisted.PayableAmount);
            Assert.Equal([750, 360], orderTransactions.Select(x => x.Debit).ToArray());
        }

        /// <summary>
        /// Проверяет, что пересчет статуса после оплаты использует сохраненный snapshot стоимости, а не текущие настройки.
        /// </summary>
        [Fact]
        public async Task PayOrder_MovesAwaitingPaymentToPending_UsingPayableAmountSnapshotAfterSettingsChange()
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("admin");
            ReviewOrderEntity order = await app.Data.CreateReviewOrderAsync(
                createdByUserId: user.Id,
                type: ReviewOrderType.Donation,
                status: ReviewOrderStatus.AwaitingPayment,
                trackDurationSeconds: 420,
                payableAmount: 1_110,
                totalPaymentAmount: 1_100);

            TransactionEntity payment = await app.RunScopeAsync(async services =>
            {
                await ConfigurePricing(services, extraTimeAmountPerSecond: 10, detailedReviewAmount: 1_000);

                return await services.GetRequiredService<ReviewOrderService>().PayOrder(new PayCommand
                {
                    ReviewOrderId = order.Id,
                    Nickname = "Nick-SnapshotStatus",
                    PaymentAmount = 10,
                    TopUpProvider = AccountTopUpProvider.Manual,
                    CreatedByUserId = user.Id,
                });
            });

            ReviewOrderEntity persisted = await app.GetOrderAsync(order.Id);

            Assert.Equal(TransactionKind.Payment, payment.Kind);
            Assert.Equal(ReviewOrderStatus.Pending, persisted.Status);
            Assert.Equal(1_110, persisted.PayableAmount);
        }

        private static async Task ConfigurePricing(
            IServiceProvider services,
            long extraTimeAmountPerSecond,
            long detailedReviewAmount)
        {
            AppSettingsService appSettingsService = services.GetRequiredService<AppSettingsService>();
            await appSettingsService.Update(new AppSettingsDto
            {
                ReviewOrderNominalAmount = appSettingsService.Settings.ReviewOrderNominalPrice,
                ReviewOrderExtraTimeAmountPerSecond = extraTimeAmountPerSecond,
                ReviewOrderDetailedReviewAmount = detailedReviewAmount,
            }, CancellationToken.None);
        }
    }
}
