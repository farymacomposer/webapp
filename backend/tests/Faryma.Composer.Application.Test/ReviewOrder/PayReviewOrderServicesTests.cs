using System.ComponentModel.DataAnnotations;
using Faryma.Composer.Application.Features.AppSettings;
using Faryma.Composer.Application.Features.ReviewOrder;
using Faryma.Composer.Application.Test.Infrastructure;
using Faryma.Composer.Contracts.Api.Features.AppSettings;
using Faryma.Composer.Contracts.Api.Features.ReviewOrder.PayDetailedReview;
using Faryma.Composer.Contracts.Application.Features.ReviewOrder.Commands;
using Faryma.Composer.Contracts.Application.Features.ReviewOrder.Models;
using Faryma.Composer.Contracts.Infrastructure.Entities;
using Faryma.Composer.Contracts.Infrastructure.Entities.TransactionSources;
using Faryma.Composer.Contracts.Infrastructure.Enums;
using Faryma.Composer.Domain.Enums;
using Microsoft.AspNetCore.Identity;

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

            PayDetailedReviewResult result = await app.RunScopeAsync(async services =>
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

            TransactionEntity payment = Assert.IsType<TransactionEntity>(result.PaymentTransaction);
            Assert.Null(result.UserEntitlementRedemption);
            Assert.Equal(order.Id, result.ReviewOrder.Id);
            Assert.Equal(TransactionKind.Payment, payment.Kind);
            Assert.Equal(source.Id, payment.TransactionSourceId);
            Assert.Equal(650, source.Price);
            Assert.Single(source.Transactions);
            Assert.Equal(650, source.Transactions.Single().Debit);
            Assert.Equal(
                [TransactionKind.AccountTopUp, TransactionKind.Payment],
                accountTransactions.Select(x => x.Kind).ToArray());
        }

        /// <summary>
        /// Проверяет, что подробный разбор можно оплатить жетоном без денежных транзакций.
        /// </summary>
        [Fact]
        public async Task PayDetailedReview_RedeemsServiceTokenWithoutPayment()
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("admin");
            ReviewOrderEntity order = await app.Data.CreateReviewOrderAsync(createdByUserId: user.Id);
            long tokenId = await app.RunScopeAsync(async services =>
            {
                UnitOfWork uow = services.GetRequiredService<UnitOfWork>();
                UserEntity actualUser = await services.GetRequiredService<UserManager<UserEntity>>()
                    .FindByIdAsync(user.Id.ToString())
                    ?? throw new InvalidOperationException("Пользователь не найден");
                UserNicknameEntity userNickname = await uow.UserNicknameStore.FindByNickname("Nick-DetailedToken")
                    ?? uow.UserNicknameStore.Create("Nick-DetailedToken");

                UserEntitlementEntity token = uow.UserEntitlementStore.Create(
                    userNickname,
                    UserEntitlementTarget.DetailedReview,
                    actualUser);

                await uow.SaveChanges();

                return token.Id;
            });

            PayDetailedReviewResult result = await app.RunScopeAsync(async services =>
            {
                await ConfigurePricing(services, extraTimeAmountPerSecond: 3, detailedReviewAmount: 650);

                return await services.GetRequiredService<ReviewOrderService>().PayDetailedReview(new PayDetailedReviewCommand
                {
                    ReviewOrderId = order.Id,
                    Nickname = "Nick-DetailedToken",
                    UserEntitlementId = tokenId,
                    CreatedByUserId = user.Id,
                });
            });

            (ReviewOrderDetailedReviewPaymentEntity source, UserEntitlementRedemptionEntity redemption, int transactionCount) =
                await app.RunScopeAsync(async services =>
                {
                    IDbContextFactory<AppDbContext> factory = services.GetRequiredService<IDbContextFactory<AppDbContext>>();
                    await using AppDbContext context = await factory.CreateDbContextAsync();
                    ReviewOrderDetailedReviewPaymentEntity source = await context.ReviewOrderDetailedReviewPayments
                        .AsNoTracking()
                        .SingleAsync(x => x.ReviewOrderId == order.Id);
                    UserEntitlementRedemptionEntity redemption = await context.UserEntitlementRedemptions
                        .AsNoTracking()
                        .SingleAsync(x => x.ReviewOrderDetailedReviewPaymentId == source.Id);
                    int transactionCount = await context.Transactions
                        .AsNoTracking()
                        .Where(x => x.TransactionSource is ReviewOrderDetailedReviewPaymentEntity)
                        .CountAsync(x => ((ReviewOrderDetailedReviewPaymentEntity)x.TransactionSource).ReviewOrderId == order.Id);

                    return (source, redemption, transactionCount);
                });

            Assert.Null(result.PaymentTransaction);
            Assert.NotNull(result.UserEntitlementRedemption);
            Assert.Equal(result.UserEntitlementRedemption.Id, redemption.Id);
            Assert.Equal(order.Id, result.ReviewOrder.Id);
            Assert.Equal(650, source.Price);
            Assert.Equal(UserEntitlementTarget.DetailedReview, redemption.Target);
            Assert.Equal(650, redemption.CoveredAmount);
            Assert.Equal(tokenId, redemption.UserEntitlementId);
            Assert.Equal(0, transactionCount);
        }

        /// <summary>
        /// Проверяет, что DTO оплаты подробного разбора принимает строго один способ оплаты.
        /// </summary>
        [Theory]
        [InlineData(null, null, false)]
        [InlineData(AccountTopUpProvider.Manual, 10L, false)]
        [InlineData(AccountTopUpProvider.Manual, null, true)]
        [InlineData(null, 10L, true)]
        public void PayDetailedReviewOrderRequest_ValidatesMoneyOrTokenExclusivity(
            AccountTopUpProvider? topUpProvider,
            long? userEntitlementId,
            bool isValid)
        {
            PayDetailedReviewOrderRequest request = new()
            {
                ReviewOrderId = 1,
                Nickname = "Nick-Detailed",
                TopUpProvider = topUpProvider,
                UserEntitlementId = userEntitlementId,
            };

            List<ValidationResult> results = Validate(request);

            Assert.Equal(isValid, results.Count == 0);
        }

        /// <summary>
        /// Проверяет, что application-сервис также не принимает смешанную или пустую оплату.
        /// </summary>
        [Theory]
        [InlineData(null, null)]
        [InlineData(AccountTopUpProvider.Manual, 10L)]
        public async Task PayDetailedReview_Throws_WhenPaymentModeIsMissingOrMixed(
            AccountTopUpProvider? topUpProvider,
            long? userEntitlementId)
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("admin");
            ReviewOrderEntity order = await app.Data.CreateReviewOrderAsync(createdByUserId: user.Id);

            await Assert.ThrowsAsync<ReviewOrderException>(() =>
                app.RunScopeAsync(async services =>
                {
                    await ConfigurePricing(services, extraTimeAmountPerSecond: 3, detailedReviewAmount: 650);

                    await services.GetRequiredService<ReviewOrderService>().PayDetailedReview(new PayDetailedReviewCommand
                    {
                        ReviewOrderId = order.Id,
                        Nickname = "Nick-Detailed",
                        TopUpProvider = topUpProvider,
                        UserEntitlementId = userEntitlementId,
                        CreatedByUserId = user.Id,
                    });
                }));
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
            await appSettingsService.Update(new AppSettingsDto
            {
                ReviewOrderNominalAmount = appSettingsService.Settings.ReviewOrderNominalPrice,
                ReviewOrderExtraTimeAmountPerSecond = extraTimeAmountPerSecond,
                ReviewOrderDetailedReviewAmount = detailedReviewAmount,
            }, CancellationToken.None);
        }

        private static List<ValidationResult> Validate(object request)
        {
            List<ValidationResult> results = [];
            Validator.TryValidateObject(
                request,
                new ValidationContext(request),
                results,
                validateAllProperties: true);

            return results;
        }
    }
}
