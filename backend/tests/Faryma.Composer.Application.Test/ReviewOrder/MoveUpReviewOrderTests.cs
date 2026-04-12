using Faryma.Composer.Application.Features.ReviewOrder;
using Faryma.Composer.Application.Test.Infrastructure;
using Faryma.Composer.Contracts.Application.Features.ReviewOrder.Commands;
using Faryma.Composer.Contracts.Infrastructure.Entities;
using Faryma.Composer.Contracts.Infrastructure.Entities.TransactionSources;
using Faryma.Composer.Contracts.Infrastructure.Enums;

namespace Faryma.Composer.Application.Test.ReviewOrder
{
    public sealed class MoveUpReviewOrderTests(PostgreSqlFixture fixture) : ApplicationTestBase(fixture)
    {
        [Theory]
        [InlineData(ReviewOrderStatus.Preorder)]
        [InlineData(ReviewOrderStatus.Pending)]
        public async Task MoveUp_AddsPayment_WhenOrderHasValidStatus(ReviewOrderStatus status)
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("admin");
            ReviewOrderEntity order = await app.Data.CreateReviewOrderAsync(
                createdByUserId: user.Id,
                status: status,
                type: ReviewOrderType.Donation,
                trackUrl: status == ReviewOrderStatus.Preorder ? null : "https://example.com/track");

            TransactionEntity payment = await app.RunScopeAsync(services =>
                services.GetRequiredService<ReviewOrderService>().MoveUp(new MoveUpCommand
                {
                    ReviewOrderId = order.Id,
                    Nickname = "Nick-Move",
                    PaymentAmount = 500,
                    TopUpProvider = AccountTopUpProvider.Manual,
                    CreatedByUserId = user.Id,
                }, CancellationToken.None));

            List<TransactionEntity> orderTransactions = await app.GetOrderTransactionsAsync(order.Id);
            int transactionCount = await app.RunScopeAsync(async services =>
            {
                IDbContextFactory<AppDbContext> factory = services.GetRequiredService<IDbContextFactory<AppDbContext>>();
                await using AppDbContext context = await factory.CreateDbContextAsync();
                return await context.Transactions.CountAsync();
            });

            Assert.Equal(TransactionKind.Payment, payment.Kind);
            Assert.Equal(order.Id, payment.TransactionSourceId);
            Assert.Single(orderTransactions);
            Assert.Equal(500, orderTransactions[0].Debit);
            Assert.Equal(2, transactionCount);
        }

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
                completedAt: status == ReviewOrderStatus.Completed ? app.FixedNow : null,
                canceledAt: status == ReviewOrderStatus.Canceled ? app.FixedNow : null,
                cancelReason: status == ReviewOrderStatus.Canceled ? "reason" : null,
                inProgressAt: status == ReviewOrderStatus.InProgress ? app.FixedNow : null);

            await Assert.ThrowsAsync<ReviewOrderException>(() =>
                app.RunScopeAsync(services =>
                    services.GetRequiredService<ReviewOrderService>().MoveUp(new MoveUpCommand
                    {
                        ReviewOrderId = order.Id,
                        Nickname = "Nick-Move",
                        PaymentAmount = 500,
                        TopUpProvider = AccountTopUpProvider.Manual,
                        CreatedByUserId = user.Id,
                    }, CancellationToken.None)));
        }
    }
}
