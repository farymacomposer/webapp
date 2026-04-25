using Faryma.Composer.Application.Features.OrderQueue;
using Faryma.Composer.Application.Features.ReviewOrder;
using Faryma.Composer.Application.Test.Infrastructure;
using Faryma.Composer.Contracts.Application.Features.OrderQueue.Models;
using Faryma.Composer.Contracts.Application.Features.ReviewOrder.Commands;
using Faryma.Composer.Contracts.Infrastructure.Entities;
using Faryma.Composer.Contracts.Infrastructure.Entities.TransactionSources;
using Faryma.Composer.Contracts.Infrastructure.Enums;

namespace Faryma.Composer.Application.Test.ReviewOrder
{
    public sealed class MoveUpReviewOrderTests(PostgreSqlFixture fixture) : ApplicationTestBase(fixture)
    {
        /// <summary>
        /// Проверяет, что доплата по заказу создает платеж для допустимого статуса.
        /// </summary>
        [Theory]
        [InlineData(ReviewOrderStatus.Preorder)]
        [InlineData(ReviewOrderStatus.Pending)]
        public async Task MoveUp_AddsPayment_WhenOrderHasValidStatus(ReviewOrderStatus status)
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("admin");
            ReviewOrderEntity order = await app.Data.CreateReviewOrderAsync(
                createdByUserId: user.Id,
                type: ReviewOrderType.Donation,
                status: status,
                trackUrl: status == ReviewOrderStatus.Preorder ? null : "https://example.com/track");

            TransactionEntity payment = await app.RunScopeAsync(services =>
                services.GetRequiredService<ReviewOrderService>().MoveUp(new MoveUpCommand
                {
                    ReviewOrderId = order.Id,
                    Nickname = "Nick-Move",
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
        public async Task MoveUp_Throws_WhenOrderHasInvalidStatus(ReviewOrderStatus status)
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
                    services.GetRequiredService<ReviewOrderService>().MoveUp(new MoveUpCommand
                    {
                        ReviewOrderId = order.Id,
                        Nickname = "Nick-Move",
                        PaymentAmount = 500,
                        TopUpProvider = AccountTopUpProvider.Manual,
                        CreatedByUserId = user.Id,
                    })));
        }

        /// <summary>
        /// Проверяет, что для несуществующего заказа выбрасывается ошибка.
        /// </summary>
        [Fact]
        public async Task MoveUp_Throws_WhenOrderDoesNotExist()
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("admin");

            await Assert.ThrowsAsync<ReviewOrderException>(() =>
                app.RunScopeAsync(services =>
                    services.GetRequiredService<ReviewOrderService>().MoveUp(new MoveUpCommand
                    {
                        ReviewOrderId = long.MaxValue,
                        Nickname = "Nick-Move",
                        PaymentAmount = 500,
                        TopUpProvider = AccountTopUpProvider.Manual,
                        CreatedByUserId = user.Id,
                    })));
        }

        /// <summary>
        /// Проверяет, что доплата улучшает позицию заказа в очереди.
        /// </summary>
        [Fact]
        public async Task MoveUp_ImprovesOrderPositionInQueue()
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
                services.GetRequiredService<ReviewOrderService>().CreateDonation(new CreateDonationOrderCommand
                {
                    Nickname = "Nick-Strong",
                    TrackUrl = "https://example.com/strong",
                    UserComment = null,
                    PaymentAmount = 1_000,
                    TopUpProvider = AccountTopUpProvider.Manual,
                    CreatedByUserId = user.Id,
                }));
            await app.DrainQueueEventsAsync();

            ReviewOrderEntity candidate = await app.RunScopeAsync(services =>
                services.GetRequiredService<ReviewOrderService>().CreateDonation(new CreateDonationOrderCommand
                {
                    Nickname = "Nick-Candidate",
                    TrackUrl = "https://example.com/candidate",
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
                services.GetRequiredService<ReviewOrderService>().MoveUp(new MoveUpCommand
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
    }
}