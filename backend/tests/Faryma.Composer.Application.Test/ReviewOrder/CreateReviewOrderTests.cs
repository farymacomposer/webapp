using System.ComponentModel.DataAnnotations;
using Faryma.Composer.Application.Features.ReviewOrder;
using Faryma.Composer.Application.Test.Infrastructure;
using Faryma.Composer.Contracts.Api.Features.ReviewOrder.Create;
using Faryma.Composer.Contracts.Application.Features.ReviewOrder.Commands;
using Faryma.Composer.Contracts.Infrastructure.Entities;
using Faryma.Composer.Contracts.Infrastructure.Entities.TransactionSources;
using Faryma.Composer.Contracts.Infrastructure.Enums;

namespace Faryma.Composer.Application.Test.ReviewOrder
{
    public sealed class CreateReviewOrderTests(PostgreSqlFixture fixture) : TestBase(fixture)
    {
        /// <summary>
        /// Проверяет, что нулевой платеж в API-запросе считается отсутствующим платежом, если есть покрытие.
        /// </summary>
        [Fact]
        public void CreateDonationReviewOrderRequest_AllowsZeroPayment_WhenDonationHasCouponCoverage()
        {
            CreateDonationReviewOrderRequest request = new()
            {
                Nickname = "Nick-Coupon",
                TrackUrl = "https://example.com/track",
                TrackDurationSeconds = 60,
                PaymentAmount = 0,
                CouponAmount = 750,
                TopUpProvider = null,
                UserComment = null,
            };

            List<ValidationResult> results = Validate(request);

            Assert.Empty(results);
        }

        /// <summary>
        /// Проверяет, что donation DTO требует платеж или покрытие.
        /// </summary>
        [Fact]
        public void CreateDonationReviewOrderRequest_RejectsMissingCoverage()
        {
            CreateDonationReviewOrderRequest request = new()
            {
                Nickname = "Nick-NoCoverage",
                TrackUrl = "https://example.com/track",
                TrackDurationSeconds = 60,
                PaymentAmount = 0,
                CouponAmount = null,
                TopUpProvider = null,
                UserComment = null,
            };

            List<ValidationResult> results = Validate(request);

            Assert.Contains(results, x => x.ErrorMessage == "Для донатных заказов требуется платеж, купон или жетон");
        }

        /// <summary>
        /// Проверяет, что out-of-queue DTO требует положительное покрытие.
        /// </summary>
        [Fact]
        public void CreateOutOfQueueReviewOrderRequest_RejectsMissingCoverage()
        {
            CreateOutOfQueueReviewOrderRequest request = new()
            {
                Nickname = "Nick-OOQ",
                TrackUrl = "https://example.com/track",
                TrackDurationSeconds = 60,
                CouponAmount = 0,
                UserComment = null,
            };

            List<ValidationResult> results = Validate(request);

            Assert.Contains(results, x => x.ErrorMessage == "Для внеочередных заказов требуется купон, жетон или админское покрытие");
        }

        /// <summary>
        /// Проверяет, что free DTO требует положительное покрытие.
        /// </summary>
        [Fact]
        public void CreateFreeReviewOrderRequest_RejectsMissingCoverage()
        {
            CreateFreeReviewOrderRequest request = new()
            {
                Nickname = "Nick-Free",
                TrackUrl = "https://example.com/track",
                TrackDurationSeconds = 60,
                CouponAmount = 0,
                UserComment = null,
            };

            List<ValidationResult> results = Validate(request);

            Assert.Contains(results, x => x.ErrorMessage == "Для бесплатных заказов требуется купон, жетон или админское покрытие");
        }

        /// <summary>
        /// Проверяет, что charity DTO не принимает платежные поля.
        /// </summary>
        [Fact]
        public void CreateCharityReviewOrderRequest_DoesNotExposePaymentOrCouponFields()
        {
            CreateCharityReviewOrderRequest request = new()
            {
                Nickname = "Nick-Charity",
                TrackUrl = "https://example.com/track",
                TrackDurationSeconds = 60,
                UserComment = null,
            };

            List<ValidationResult> results = Validate(request);

            Assert.Empty(results);
            Assert.Null(typeof(CreateCharityReviewOrderRequest).GetProperty("PaymentAmount"));
            Assert.Null(typeof(CreateCharityReviewOrderRequest).GetProperty("CouponAmount"));
        }

        /// <summary>
        /// Проверяет, что donation-заказ со ссылкой создается сразу в Pending и с платежом.
        /// </summary>
        [Fact]
        public async Task CreateDonation_CreatesPendingOrderAndPayment_WhenTrackUrlProvided()
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("admin");
            ComposerStreamEntity stream = await app.Data.CreateStreamAsync(
                createdByUserId: user.Id,
                eventDate: app.Today.AddDays(1),
                type: ComposerStreamType.Donation);

            ReviewOrderEntity order = await app.RunScopeAsync(services =>
                services.GetRequiredService<ReviewOrderService>().CreateDonation(new CreateDonationOrderCommand
                {
                    Nickname = "Nick-Donation",
                    TrackUrl = "https://example.com/track",
                    TrackDurationSeconds = 60,
                    UserComment = "комментарий",
                    PaymentAmount = 1_200,
                    TopUpProvider = AccountTopUpProvider.Manual,
                    CreatedByUserId = user.Id,
                }));

            ReviewOrderEntity persisted = await app.GetOrderAsync(order.Id);
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

            Assert.Equal(ReviewOrderType.Donation, persisted.Type);
            Assert.Equal(ReviewOrderStatus.Pending, persisted.Status);
            Assert.Equal(stream.Id, persisted.CreationStreamId);
            Assert.Equal("https://example.com/track", persisted.TrackUrl);
            Assert.Single(orderTransactions);
            Assert.Equal(TransactionKind.Payment, orderTransactions[0].Kind);
            Assert.Equal(1_200, orderTransactions[0].Debit);
            Assert.Equal(
                [TransactionKind.AccountTopUp, TransactionKind.Payment],
                accountTransactions.Select(x => x.Kind).ToArray());
        }

        /// <summary>
        /// Проверяет, что donation-заказ без ссылки создается как предзаказ.
        /// </summary>
        [Fact]
        public async Task CreateDonation_CreatesPreorder_WhenTrackUrlIsMissing()
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("admin");
            ComposerStreamEntity stream = await app.Data.CreateStreamAsync(
                createdByUserId: user.Id,
                eventDate: app.Today.AddDays(1),
                type: ComposerStreamType.Donation);

            ReviewOrderEntity order = await app.RunScopeAsync(services =>
                services.GetRequiredService<ReviewOrderService>().CreateDonation(new CreateDonationOrderCommand
                {
                    Nickname = "Nick-Preorder",
                    TrackUrl = null,
                    TrackDurationSeconds = null,
                    UserComment = null,
                    PaymentAmount = 900,
                    TopUpProvider = AccountTopUpProvider.Manual,
                    CreatedByUserId = user.Id,
                }));

            Assert.Equal(stream.Id, order.CreationStreamId);
            Assert.Equal(ReviewOrderStatus.Preorder, order.Status);
            Assert.Null(order.TrackUrl);
        }

        /// <summary>
        /// Проверяет, что для нового ника выбирается ближайший доступный стрим.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task CreateDonation_UsesNearestAvailableStream_WhenNicknameHasNoHistory(bool withTrackUrl)
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("admin");
            ComposerStreamEntity nearestCharity = await app.Data.CreateStreamAsync(
                createdByUserId: user.Id,
                eventDate: app.Today,
                type: ComposerStreamType.Charity);
            await app.Data.CreateStreamAsync(
                createdByUserId: user.Id,
                eventDate: app.Today.AddDays(1),
                type: ComposerStreamType.Donation);

            ReviewOrderEntity order = await app.RunScopeAsync(services =>
                services.GetRequiredService<ReviewOrderService>().CreateDonation(new CreateDonationOrderCommand
                {
                    Nickname = "Nick-NewDonation",
                    TrackUrl = withTrackUrl ? "https://example.com/new-donation" : null,
                    TrackDurationSeconds = withTrackUrl ? 60 : null,
                    UserComment = null,
                    PaymentAmount = 1_000,
                    TopUpProvider = AccountTopUpProvider.Manual,
                    CreatedByUserId = user.Id,
                }));

            Assert.Equal(nearestCharity.Id, order.CreationStreamId);
            Assert.Equal(withTrackUrl ? ReviewOrderStatus.Pending : ReviewOrderStatus.Preorder, order.Status);
        }

        /// <summary>
        /// Проверяет, что free-заказ для знакомого ника привязывается к donation-стриму.
        /// </summary>
        [Fact]
        public async Task CreateFree_UsesNearestDonationStream_WhenNicknameAlreadyHasOrders()
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("admin");
            ComposerStreamEntity nearerCharity = await app.Data.CreateStreamAsync(
                createdByUserId: user.Id,
                eventDate: app.Today,
                type: ComposerStreamType.Charity);
            ComposerStreamEntity donationStream = await app.Data.CreateStreamAsync(
                createdByUserId: user.Id,
                eventDate: app.Today.AddDays(1),
                type: ComposerStreamType.Donation);

            await app.Data.CreateReviewOrderAsync(
                createdByUserId: user.Id,
                creationStreamId: donationStream.Id,
                nickname: "Nick-Free",
                type: ReviewOrderType.Donation,
                status: ReviewOrderStatus.Pending,
                totalPaymentAmount: 900);

            ReviewOrderEntity order = await app.RunScopeAsync(services =>
                services.GetRequiredService<ReviewOrderService>().CreateFree(new CreateFreeOrderCommand
                {
                    Nickname = "Nick-Free",
                    TrackUrl = "https://example.com/free",
                    TrackDurationSeconds = 60,
                    UserComment = null,
                    CouponAmount = 1_000,
                    CreatedByUserId = user.Id,
                }));
            ReviewOrderEntity persisted = await app.GetOrderAsync(order.Id);

            Assert.NotEqual(nearerCharity.Id, order.CreationStreamId);
            Assert.Equal(donationStream.Id, order.CreationStreamId);
            Assert.Equal(ReviewOrderType.Free, order.Type);
            Assert.Equal(0, persisted.NonPaymentCoverageAmount);
            UserEntitlementRedemptionEntity redemption = Assert.Single(persisted.CoverageRedemptions);
            Assert.Equal(UserEntitlementTarget.ReviewOrder, redemption.Target);
            Assert.Equal(1_000, redemption.CoveredAmount);
        }

        /// <summary>
        /// Проверяет, что donation-заказ для знакомого ника привязывается к donation-стриму.
        /// </summary>
        [Fact]
        public async Task CreateDonation_UsesNearestDonationStream_WhenNicknameAlreadyHasOrders()
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("admin");
            ComposerStreamEntity nearerCharity = await app.Data.CreateStreamAsync(
                createdByUserId: user.Id,
                eventDate: app.Today,
                type: ComposerStreamType.Charity);
            ComposerStreamEntity donationStream = await app.Data.CreateStreamAsync(
                createdByUserId: user.Id,
                eventDate: app.Today.AddDays(1),
                type: ComposerStreamType.Donation);

            await app.Data.CreateReviewOrderAsync(
                createdByUserId: user.Id,
                creationStreamId: donationStream.Id,
                nickname: "Nick-DonationHistory",
                type: ReviewOrderType.Donation,
                status: ReviewOrderStatus.Pending,
                totalPaymentAmount: 900);

            ReviewOrderEntity order = await app.RunScopeAsync(services =>
                services.GetRequiredService<ReviewOrderService>().CreateDonation(new CreateDonationOrderCommand
                {
                    Nickname = "Nick-DonationHistory",
                    TrackUrl = "https://example.com/donation-repeat",
                    TrackDurationSeconds = 60,
                    UserComment = null,
                    PaymentAmount = 600,
                    TopUpProvider = AccountTopUpProvider.Manual,
                    CreatedByUserId = user.Id,
                }));

            Assert.NotEqual(nearerCharity.Id, order.CreationStreamId);
            Assert.Equal(donationStream.Id, order.CreationStreamId);
            Assert.Equal(ReviewOrderType.Donation, order.Type);
        }

        /// <summary>
        /// Проверяет, что free-заказ для нового ника привязывается к ближайшему доступному стриму.
        /// </summary>
        [Fact]
        public async Task CreateFree_UsesNearestAvailableStream_WhenNicknameHasNoHistory()
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("admin");
            ComposerStreamEntity nearestCharity = await app.Data.CreateStreamAsync(
                createdByUserId: user.Id,
                eventDate: app.Today,
                type: ComposerStreamType.Charity);
            await app.Data.CreateStreamAsync(
                createdByUserId: user.Id,
                eventDate: app.Today.AddDays(1),
                type: ComposerStreamType.Donation);

            ReviewOrderEntity order = await app.RunScopeAsync(services =>
                services.GetRequiredService<ReviewOrderService>().CreateFree(new CreateFreeOrderCommand
                {
                    Nickname = "Nick-NewFree",
                    TrackUrl = "https://example.com/free",
                    TrackDurationSeconds = 60,
                    UserComment = null,
                    CouponAmount = 1_000,
                    CreatedByUserId = user.Id,
                }));

            Assert.Equal(nearestCharity.Id, order.CreationStreamId);
            Assert.Equal(ReviewOrderStatus.Pending, order.Status);
        }

        /// <summary>
        /// Проверяет, что out-of-queue заказ создается на ближайшем доступном стриме.
        /// </summary>
        [Fact]
        public async Task CreateOutOfQueue_UsesNearestAvailableStream()
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("admin");
            ComposerStreamEntity nearest = await app.Data.CreateStreamAsync(
                createdByUserId: user.Id,
                eventDate: app.Today,
                type: ComposerStreamType.Charity);
            await app.Data.CreateStreamAsync(
                createdByUserId: user.Id,
                eventDate: app.Today.AddDays(2),
                type: ComposerStreamType.Donation);

            ReviewOrderEntity order = await app.RunScopeAsync(services =>
                services.GetRequiredService<ReviewOrderService>().CreateOutOfQueue(new CreateOutOfQueueOrderCommand
                {
                    Nickname = "Nick-OOQ",
                    TrackUrl = null,
                    TrackDurationSeconds = null,
                    UserComment = null,
                    CouponAmount = 1_000,
                    CreatedByUserId = user.Id,
                }));
            ReviewOrderEntity persisted = await app.GetOrderAsync(order.Id);

            Assert.Equal(nearest.Id, order.CreationStreamId);
            Assert.Equal(ReviewOrderType.OutOfQueue, order.Type);
            Assert.Equal(ReviewOrderStatus.Preorder, order.Status);
            Assert.Equal(1_000, order.PayableAmount);
            Assert.Equal(0, persisted.NonPaymentCoverageAmount);
            UserEntitlementRedemptionEntity redemption = Assert.Single(persisted.CoverageRedemptions);
            Assert.Equal(UserEntitlementTarget.OutOfQueueReviewOrder, redemption.Target);
            Assert.Equal(1_000, redemption.CoveredAmount);
        }

        /// <summary>
        /// Проверяет, что donation-заказ с частичным покрытием создается в ожидании оплаты.
        /// </summary>
        [Fact]
        public async Task CreateDonation_CreatesAwaitingPayment_WhenKnownTrackIsPartiallyCovered()
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("admin");
            await app.Data.CreateStreamAsync(
                createdByUserId: user.Id,
                eventDate: app.Today.AddDays(1),
                type: ComposerStreamType.Donation);

            ReviewOrderEntity order = await app.RunScopeAsync(services =>
                services.GetRequiredService<ReviewOrderService>().CreateDonation(new CreateDonationOrderCommand
                {
                    Nickname = "Nick-Partial",
                    TrackUrl = "https://example.com/partial",
                    TrackDurationSeconds = 60,
                    UserComment = null,
                    PaymentAmount = 600,
                    TopUpProvider = AccountTopUpProvider.Manual,
                    CreatedByUserId = user.Id,
                }));

            Assert.Equal(ReviewOrderStatus.AwaitingPayment, order.Status);
            Assert.Equal(600, order.GetTotalAmount());
        }

        /// <summary>
        /// Проверяет, что donation-заказ нельзя создать без платежа или купона.
        /// </summary>
        [Fact]
        public async Task CreateDonation_Throws_WhenCoverageIsMissing()
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("admin");
            await app.Data.CreateStreamAsync(
                createdByUserId: user.Id,
                eventDate: app.Today.AddDays(1),
                type: ComposerStreamType.Donation);

            await Assert.ThrowsAsync<ReviewOrderException>(() =>
                app.RunScopeAsync(services =>
                    services.GetRequiredService<ReviewOrderService>().CreateDonation(new CreateDonationOrderCommand
                    {
                        Nickname = "Nick-NoCoverage",
                        TrackUrl = "https://example.com/no-coverage",
                        TrackDurationSeconds = 60,
                        UserComment = null,
                        CreatedByUserId = user.Id,
                    })));
        }

        /// <summary>
        /// Проверяет, что купон покрывает готовность заказа без денежного платежа.
        /// </summary>
        [Fact]
        public async Task CreateDonation_CreatesPendingWithoutPayment_WhenCouponCoversRequiredAmount()
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("admin");
            await app.Data.CreateStreamAsync(
                createdByUserId: user.Id,
                eventDate: app.Today.AddDays(1),
                type: ComposerStreamType.Donation);

            ReviewOrderEntity order = await app.RunScopeAsync(services =>
                services.GetRequiredService<ReviewOrderService>().CreateDonation(new CreateDonationOrderCommand
                {
                    Nickname = "Nick-Coupon",
                    TrackUrl = "https://example.com/coupon",
                    TrackDurationSeconds = 60,
                    UserComment = null,
                    CouponAmount = 1_000,
                    CreatedByUserId = user.Id,
                }));

            List<TransactionEntity> orderTransactions = await app.GetOrderTransactionsAsync(order.Id);
            ReviewOrderEntity persisted = await app.GetOrderAsync(order.Id);

            Assert.Equal(ReviewOrderStatus.Pending, order.Status);
            Assert.Equal(0, persisted.NonPaymentCoverageAmount);
            UserEntitlementRedemptionEntity redemption = Assert.Single(persisted.CoverageRedemptions);
            UserEntitlementEntity entitlement = await app.RunScopeAsync(async services =>
            {
                IDbContextFactory<AppDbContext> factory = services.GetRequiredService<IDbContextFactory<AppDbContext>>();
                await using AppDbContext context = await factory.CreateDbContextAsync();

                return await context.UserEntitlements
                    .AsNoTracking()
                    .SingleAsync(x => x.Id == redemption.UserEntitlementId);
            });

            Assert.Equal(UserEntitlementKind.AmountCoupon, entitlement.Kind);
            Assert.Equal(UserEntitlementTarget.ReviewOrder, entitlement.Target);
            Assert.Equal(1_000, entitlement.Amount);
            Assert.NotNull(entitlement.RedeemedAt);
            Assert.Equal(UserEntitlementTarget.ReviewOrder, redemption.Target);
            Assert.Equal(1_000, redemption.CoveredAmount);
            Assert.Empty(orderTransactions);
        }

        /// <summary>
        /// Проверяет, что charity-заказ создается на активном charity-стриме.
        /// </summary>
        [Fact]
        public async Task CreateCharity_UsesLiveCharityStream()
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("admin");
            ComposerStreamEntity charityStream = await app.Data.CreateStreamAsync(
                createdByUserId: user.Id,
                eventDate: app.Today,
                type: ComposerStreamType.Charity,
                status: ComposerStreamStatus.Live,
                startedAt: app.FixedNow);

            ReviewOrderEntity order = await app.RunScopeAsync(services =>
                services.GetRequiredService<ReviewOrderService>().CreateCharity(new CreateCharityOrderCommand
                {
                    Nickname = "Nick-Charity",
                    TrackUrl = "https://example.com/charity",
                    TrackDurationSeconds = 60,
                    UserComment = null,
                    CreatedByUserId = user.Id,
                }));

            Assert.Equal(charityStream.Id, order.CreationStreamId);
            Assert.Equal(ReviewOrderType.Charity, order.Type);
            Assert.Equal(0, order.PayableAmount);
        }

        /// <summary>
        /// Проверяет, что charity-заказ нельзя создать без активного charity-стрима.
        /// </summary>
        [Fact]
        public async Task CreateCharity_Throws_WhenLiveCharityStreamIsMissing()
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("admin");
            await app.Data.CreateStreamAsync(
                createdByUserId: user.Id,
                eventDate: app.Today,
                type: ComposerStreamType.Donation,
                status: ComposerStreamStatus.Live,
                startedAt: app.FixedNow);

            await Assert.ThrowsAsync<ReviewOrderException>(() =>
                app.RunScopeAsync(services =>
                    services.GetRequiredService<ReviewOrderService>().CreateCharity(new CreateCharityOrderCommand
                    {
                        Nickname = "Nick-Charity",
                        TrackUrl = "https://example.com/charity",
                        TrackDurationSeconds = 60,
                        UserComment = null,
                        CreatedByUserId = user.Id,
                    })));
        }

        /// <summary>
        /// Проверяет, что создание заказа падает, если не найден подходящий стрим.
        /// </summary>
        [Theory]
        [InlineData("Donation")]
        [InlineData("Free")]
        [InlineData("OutOfQueue")]
        public async Task CreateOrder_Throws_WhenNoSuitableStreamExists(string kind)
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("admin");

            Task action = kind switch
            {
                "Donation" => app.RunScopeAsync(services =>
                    services.GetRequiredService<ReviewOrderService>().CreateDonation(new CreateDonationOrderCommand
                    {
                        Nickname = "Nick-NoStream",
                        TrackUrl = "https://example.com/donation",
                        TrackDurationSeconds = 60,
                        UserComment = null,
                        PaymentAmount = 700,
                        TopUpProvider = AccountTopUpProvider.Manual,
                        CreatedByUserId = user.Id,
                    })),
                "Free" => app.RunScopeAsync(services =>
                    services.GetRequiredService<ReviewOrderService>().CreateFree(new CreateFreeOrderCommand
                    {
                        Nickname = "Nick-NoStream",
                        TrackUrl = "https://example.com/free",
                        TrackDurationSeconds = 60,
                        UserComment = null,
                        CouponAmount = 1_000,
                        CreatedByUserId = user.Id,
                    })),
                "OutOfQueue" => app.RunScopeAsync(services =>
                    services.GetRequiredService<ReviewOrderService>().CreateOutOfQueue(new CreateOutOfQueueOrderCommand
                    {
                        Nickname = "Nick-NoStream",
                        TrackUrl = null,
                        TrackDurationSeconds = null,
                        UserComment = null,
                        CouponAmount = 1_000,
                        CreatedByUserId = user.Id,
                    })),
                _ => throw new InvalidOperationException($"Неподдерживаемый тип: {kind}")
            };

            await Assert.ThrowsAsync<ReviewOrderException>(() => action);
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
