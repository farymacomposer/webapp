using Faryma.Composer.Application.Features.AppSettings;
using Faryma.Composer.Application.Features.ReviewOrder;
using Faryma.Composer.Application.Test.Infrastructure;
using Faryma.Composer.Contracts.Application.Features.AppSettings;
using Faryma.Composer.Contracts.Application.Features.ReviewOrder.Commands;
using Faryma.Composer.Contracts.Infrastructure.Entities;
using Faryma.Composer.Contracts.Infrastructure.Entities.TransactionSources;
using Faryma.Composer.Contracts.Infrastructure.Enums;

namespace Faryma.Composer.Application.Test.ReviewOrder
{
    public sealed class PayReviewOrderServicesTests(PostgreSqlFixture fixture) : TestBase(fixture)
    {
        /// <summary>
        /// Проверяет, что оплата подробного разбора создает отдельный источник платежа.
        /// </summary>
        [Fact]
        public async Task PayDetailedReview_CreatesServiceSourceTopUpAndPayment()
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("admin");
            ReviewOrderEntity order = await app.Data.CreateReviewOrderAsync(createdByUserId: user.Id);

            TransactionEntity payment = await app.RunScopeAsync(async services =>
            {
                await ConfigurePricing(services, extraTimeAmountPerSecond: 3, detailedReviewAmount: 650);

                return await services.GetRequiredService<ReviewOrderService>().PayDetailedReview(new PayDetailedReviewCommand
                {
                    ReviewOrderId = order.Id,
                    Nickname = "Nick-Detailed",
                    TopUpProvider = AccountTopUpProvider.Manual,
                    CreatedByUserId = user.Id,
                });
            });

            (ReviewOrderDetailedReviewPaymentEntity source, List<TransactionEntity> accountTransactions) =
                await app.RunScopeAsync(async services =>
                {
                    IDbContextFactory<AppDbContext> factory = services.GetRequiredService<IDbContextFactory<AppDbContext>>();
                    await using AppDbContext context = await factory.CreateDbContextAsync();
                    ReviewOrderDetailedReviewPaymentEntity source = await context.ReviewOrderDetailedReviewPayments
                        .Include(x => x.Transactions)
                        .AsNoTracking()
                        .SingleAsync(x => x.ReviewOrderId == order.Id);
                    Guid accountId = source.Transactions.Single().UserNicknameAccountId;
                    List<TransactionEntity> accountTransactions = await context.Transactions
                        .AsNoTracking()
                        .Where(x => x.UserNicknameAccountId == accountId)
                        .OrderBy(x => x.Id)
                        .ToListAsync();

                    return (source, accountTransactions);
                });

            Assert.Equal(TransactionKind.Payment, payment.Kind);
            Assert.Equal(source.Id, payment.TransactionSourceId);
            Assert.Equal(650, source.Amount);
            Assert.Single(source.Transactions);
            Assert.Equal(650, source.Transactions.Single().Debit);
            Assert.Equal(
                [TransactionKind.AccountTopUp, TransactionKind.Payment],
                accountTransactions.Select(x => x.Kind).ToArray());
        }

        /// <summary>
        /// Проверяет, что подробный разбор нельзя оплатить повторно.
        /// </summary>
        [Fact]
        public async Task PayDetailedReview_Throws_WhenAlreadyPaid()
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("admin");
            ReviewOrderEntity order = await app.Data.CreateReviewOrderAsync(createdByUserId: user.Id);

            await app.RunScopeAsync(async services =>
            {
                await ConfigurePricing(services, extraTimeAmountPerSecond: 3, detailedReviewAmount: 650);
                ReviewOrderService service = services.GetRequiredService<ReviewOrderService>();

                await service.PayDetailedReview(new PayDetailedReviewCommand
                {
                    ReviewOrderId = order.Id,
                    Nickname = "Nick-Detailed",
                    TopUpProvider = AccountTopUpProvider.Manual,
                    CreatedByUserId = user.Id,
                });
            });

            await Assert.ThrowsAsync<ReviewOrderException>(() =>
                app.RunScopeAsync(async services =>
                {
                    await ConfigurePricing(services, extraTimeAmountPerSecond: 3, detailedReviewAmount: 650);

                    await services.GetRequiredService<ReviewOrderService>().PayDetailedReview(new PayDetailedReviewCommand
                    {
                        ReviewOrderId = order.Id,
                        Nickname = "Nick-Detailed",
                        TopUpProvider = AccountTopUpProvider.Manual,
                        CreatedByUserId = user.Id,
                    });
                }));

            (int sourceCount, int transactionCount) = await app.RunScopeAsync(async services =>
            {
                IDbContextFactory<AppDbContext> factory = services.GetRequiredService<IDbContextFactory<AppDbContext>>();
                await using AppDbContext context = await factory.CreateDbContextAsync();

                int sourceCount = await context.ReviewOrderDetailedReviewPayments
                    .AsNoTracking()
                    .CountAsync(x => x.ReviewOrderId == order.Id);
                int transactionCount = await context.Transactions
                    .AsNoTracking()
                    .Where(x => x.TransactionSource is ReviewOrderDetailedReviewPaymentEntity)
                    .CountAsync(x => ((ReviewOrderDetailedReviewPaymentEntity)x.TransactionSource).ReviewOrderId == order.Id);

                return (sourceCount, transactionCount);
            });

            Assert.Equal(1, sourceCount);
            Assert.Equal(1, transactionCount);
        }

        /// <summary>
        /// Проверяет, что очередь получает заказы с платежами дополнительных услуг для расчета общей суммы.
        /// </summary>
        [Fact]
        public async Task GetOrdersInQueue_LoadsServicePaymentsForTotalAmount()
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("admin");
            ReviewOrderEntity order = await app.Data.CreateReviewOrderAsync(
                createdByUserId: user.Id,
                totalPaymentAmount: 750);

            await app.RunScopeAsync(async services =>
            {
                await ConfigurePricing(services, extraTimeAmountPerSecond: 3, detailedReviewAmount: 650);
                ReviewOrderService service = services.GetRequiredService<ReviewOrderService>();

                await service.PayDetailedReview(new PayDetailedReviewCommand
                {
                    ReviewOrderId = order.Id,
                    Nickname = "Nick-Detailed",
                    TopUpProvider = AccountTopUpProvider.Manual,
                    CreatedByUserId = user.Id,
                });
            });

            long totalAmount = await app.RunScopeAsync(async services =>
            {
                List<ReviewOrderEntity> orders = await services.GetRequiredService<UnitOfWork>()
                    .ReviewOrderQueries
                    .GetOrdersInQueue();

                return orders.Single(x => x.Id == order.Id).GetTotalAmount();
            });

            Assert.Equal(1400, totalAmount);
        }

        private static async Task ConfigurePricing(
            IServiceProvider services,
            long extraTimeAmountPerSecond,
            long detailedReviewAmount)
        {
            AppSettingsService appSettingsService = services.GetRequiredService<AppSettingsService>();
            await appSettingsService.Update(new AppSettingsModel
            {
                ReviewOrderNominalAmount = appSettingsService.Settings.ReviewOrderNominalAmount,
                ReviewOrderExtraTimeAmountPerSecond = extraTimeAmountPerSecond,
                ReviewOrderDetailedReviewAmount = detailedReviewAmount,
            }, CancellationToken.None);
        }
    }
}
