using System.ComponentModel.DataAnnotations;
using Faryma.Composer.Application.Features.ReviewOrder;
using Faryma.Composer.Application.Test.Infrastructure;
using Faryma.Composer.Contracts.Api.Features.ReviewOrder.Create;
using Faryma.Composer.Contracts.Application.Features.ReviewOrder.Commands;
using Faryma.Composer.Contracts.Infrastructure.Entities;
using Faryma.Composer.Contracts.Infrastructure.Entities.TransactionSources;
using Faryma.Composer.Contracts.Infrastructure.Enums;
using Faryma.Composer.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace Faryma.Composer.Application.Test.ReviewOrder
{
    public sealed class CreateReviewOrderTests(PostgreSqlFixture fixture) : TestBase(fixture)
    {
        /// <summary>
        /// Проверяет, что donation DTO требует денежный платеж.
        /// </summary>
        [Fact]
        public void CreateDonationReviewOrderRequest_RejectsMissingPayment()
        {
            CreateDonationReviewOrderRequest request = new()
            {
                UserNickname = "Nick-NoPayment",
                TrackUrl = "https://example.com/track",
                TrackDurationSeconds = 60,
                PaymentAmount = 0,
                TopUpProvider = AccountTopUpProvider.Manual,
                UserComment = null,
            };

            List<ValidationResult> results = Validate(request);

            Assert.Contains(results, x => x.ErrorMessage == "Для донатных заказов требуется платеж");
        }

        /// <summary>
        /// Проверяет, что donation DTO больше не принимает купонное покрытие.
        /// </summary>
        [Fact]
        public void CreateDonationReviewOrderRequest_DoesNotExposeCouponAmount()
        {
            CreateDonationReviewOrderRequest request = new()
            {
                UserNickname = "Nick-Donation",
                TrackUrl = "https://example.com/track",
                TrackDurationSeconds = 60,
                PaymentAmount = 1_000,
                TopUpProvider = AccountTopUpProvider.Manual,
                UserComment = null,
            };

            List<ValidationResult> results = Validate(request);

            Assert.Empty(results);
            Assert.Null(typeof(CreateDonationReviewOrderRequest).GetProperty("CouponAmount"));
        }

        /// <summary>
        /// Проверяет, что out-of-queue DTO больше не принимает сумму покрытия.
        /// </summary>
        [Fact]
        public void CreateOutOfQueueReviewOrderRequest_DoesNotExposeCouponAmount()
        {
            CreateOutOfQueueReviewOrderRequest request = new()
            {
                UserNickname = "Nick-OOQ",
                TrackUrl = "https://example.com/track",
                TrackDurationSeconds = 60,
                UserComment = null,
            };

            List<ValidationResult> results = Validate(request);

            Assert.Empty(results);
            Assert.Null(typeof(CreateOutOfQueueReviewOrderRequest).GetProperty("CouponAmount"));
        }

        /// <summary>
        /// Проверяет, что free DTO больше не принимает сумму покрытия.
        /// </summary>
        [Fact]
        public void CreateFreeReviewOrderRequest_DoesNotExposeCouponAmount()
        {
            CreateFreeReviewOrderRequest request = new()
            {
                UserNickname = "Nick-Free",
                TrackUrl = "https://example.com/track",
                TrackDurationSeconds = 60,
                UserComment = null,
            };

            List<ValidationResult> results = Validate(request);

            Assert.Empty(results);
            Assert.Null(typeof(CreateFreeReviewOrderRequest).GetProperty("CouponAmount"));
        }

        /// <summary>
        /// Проверяет, что charity DTO не принимает платежные поля.
        /// </summary>
        [Fact]
        public void CreateCharityReviewOrderRequest_DoesNotExposePaymentOrCouponFields()
        {
            CreateCharityReviewOrderRequest request = new()
            {
                UserNickname = "Nick-Charity",
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
                    UserNickname = "Nick-Donation",
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
                    UserNickname = "Nick-Preorder",
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
                    UserNickname = "Nick-NewDonation",
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
                    UserNickname = "Nick-Free",
                    TrackUrl = "https://example.com/free",
                    TrackDurationSeconds = 60,
                    UserComment = null,
                    CreatedByUserId = user.Id,
                }));
            ReviewOrderEntity persisted = await app.GetOrderAsync(order.Id);

            Assert.NotEqual(nearerCharity.Id, order.CreationStreamId);
            Assert.Equal(donationStream.Id, order.CreationStreamId);
            Assert.Equal(ReviewOrderType.Free, order.Type);
            UserEntitlementRedemptionEntity redemption = Assert.NotNull(persisted.CoverageRedemption);
            Assert.Equal(UserEntitlementTarget.FreeReviewOrder, redemption.Target);
            Assert.Equal(1_000, redemption.CoveredAmount);
            UserEntitlementEntity entitlement = await app.RunScopeAsync(async services =>
            {
                IDbContextFactory<AppDbContext> factory = services.GetRequiredService<IDbContextFactory<AppDbContext>>();
                await using AppDbContext context = await factory.CreateDbContextAsync();

                return await context.UserEntitlements
                    .AsNoTracking()
                    .SingleAsync(x => x.Id == redemption.UserEntitlementId);
            });
            Assert.Equal(UserEntitlementKind.ServiceToken, entitlement.Kind);
            Assert.Equal(0, entitlement.Amount);
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
                    UserNickname = "Nick-DonationHistory",
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
                    UserNickname = "Nick-NewFree",
                    TrackUrl = "https://example.com/free",
                    TrackDurationSeconds = 60,
                    UserComment = null,
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
                    UserNickname = "Nick-OOQ",
                    TrackUrl = null,
                    TrackDurationSeconds = null,
                    UserComment = null,
                    CreatedByUserId = user.Id,
                }));
            ReviewOrderEntity persisted = await app.GetOrderAsync(order.Id);

            Assert.Equal(nearest.Id, order.CreationStreamId);
            Assert.Equal(ReviewOrderType.OutOfQueue, order.Type);
            Assert.Equal(ReviewOrderStatus.Preorder, order.Status);
            Assert.Equal(1_000, order.PayableAmount);
            Assert.Equal(0, persisted.NonPaymentCoverageAmount);
            UserEntitlementRedemptionEntity redemption = Assert.NotNull(persisted.CoverageRedemption);
            Assert.Equal(UserEntitlementTarget.OutOfQueueReviewOrder, redemption.Target);
            Assert.Equal(1_000, redemption.CoveredAmount);
        }

        /// <summary>
        /// Проверяет, что пользовательский жетон обычного заказа создает бесплатный заказ и сразу погашается.
        /// </summary>
        [Fact]
        public async Task CreateWithToken_CreatesFreeOrderAndRedeemsReviewOrderToken()
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("user-token-free");
            await app.Data.CreateStreamAsync(
                createdByUserId: user.Id,
                eventDate: app.Today.AddDays(1),
                type: ComposerStreamType.Donation);
            long tokenId = await CreateServiceToken(app, user, "Nick-UserFree", UserEntitlementTarget.FreeReviewOrder);

            ReviewOrderEntity order = await app.RunScopeAsync(services =>
                services.GetRequiredService<ReviewOrderService>().CreateWithToken(new CreateTokenOrderCommand
                {
                    UserNickname = "Nick-UserFree",
                    TrackUrl = "https://example.com/user-free",
                    TrackDurationSeconds = 60,
                    UserComment = null,
                    UserEntitlementId = tokenId,
                    CreatedByUserId = user.Id,
                }));
            ReviewOrderEntity persisted = await app.GetOrderAsync(order.Id);

            Assert.Equal(ReviewOrderType.Free, persisted.Type);
            Assert.Equal(ReviewOrderStatus.Pending, persisted.Status);
            Assert.Equal(1_000, persisted.PayableAmount);
            UserEntitlementRedemptionEntity redemption = Assert.NotNull(persisted.CoverageRedemption);
            Assert.Equal(tokenId, redemption.UserEntitlementId);
            Assert.Equal(UserEntitlementTarget.FreeReviewOrder, redemption.Target);
            Assert.Equal(1_000, redemption.CoveredAmount);
            Assert.NotNull(await GetTokenRedeemedAt(app, tokenId));
        }

        /// <summary>
        /// Проверяет, что пользовательский жетон внеочередного заказа сам выбирает тип OutOfQueue.
        /// </summary>
        [Fact]
        public async Task CreateWithToken_CreatesOutOfQueueOrderFromOutOfQueueToken()
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("user-token-ooq");
            await app.Data.CreateStreamAsync(
                createdByUserId: user.Id,
                eventDate: app.Today,
                type: ComposerStreamType.Charity);
            long tokenId = await CreateServiceToken(app, user, "Nick-UserOOQ", UserEntitlementTarget.OutOfQueueReviewOrder);

            ReviewOrderEntity order = await app.RunScopeAsync(services =>
                services.GetRequiredService<ReviewOrderService>().CreateWithToken(new CreateTokenOrderCommand
                {
                    UserNickname = "Nick-UserOOQ",
                    TrackUrl = null,
                    TrackDurationSeconds = null,
                    UserComment = null,
                    UserEntitlementId = tokenId,
                    CreatedByUserId = user.Id,
                }));
            ReviewOrderEntity persisted = await app.GetOrderAsync(order.Id);

            Assert.Equal(ReviewOrderType.OutOfQueue, persisted.Type);
            Assert.Equal(ReviewOrderStatus.Preorder, persisted.Status);
            UserEntitlementRedemptionEntity redemption = Assert.NotNull(persisted.CoverageRedemption);
            Assert.Equal(UserEntitlementTarget.OutOfQueueReviewOrder, redemption.Target);
            Assert.Equal(tokenId, redemption.UserEntitlementId);
        }

        /// <summary>
        /// Проверяет, что пользовательский сценарий не принимает жетон подробного разбора для создания заказа.
        /// </summary>
        [Fact]
        public async Task CreateWithToken_Throws_WhenTokenTargetIsDetailedReview()
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("user-token-detailed");
            await app.Data.CreateStreamAsync(
                createdByUserId: user.Id,
                eventDate: app.Today.AddDays(1),
                type: ComposerStreamType.Donation);
            long tokenId = await CreateServiceToken(app, user, "Nick-UserDetailed", UserEntitlementTarget.DetailedReview);

            await Assert.ThrowsAsync<ReviewOrderException>(() =>
                app.RunScopeAsync(services =>
                    services.GetRequiredService<ReviewOrderService>().CreateWithToken(new CreateTokenOrderCommand
                    {
                        UserNickname = "Nick-UserDetailed",
                        TrackUrl = "https://example.com/user-detailed",
                        TrackDurationSeconds = 60,
                        UserComment = null,
                        UserEntitlementId = tokenId,
                        CreatedByUserId = user.Id,
                    })));
        }

        /// <summary>
        /// Проверяет, что пользовательский сценарий не принимает жетон другого пользователя.
        /// </summary>
        [Fact]
        public async Task CreateWithToken_Throws_WhenTokenBelongsToAnotherUser()
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity owner = await app.Data.CreateUserAsync("user-token-owner");
            UserEntity requester = await app.Data.CreateUserAsync("user-token-requester");
            await app.Data.CreateStreamAsync(
                createdByUserId: owner.Id,
                eventDate: app.Today.AddDays(1),
                type: ComposerStreamType.Donation);
            long tokenId = await CreateServiceToken(app, owner, "Nick-OtherOwner", UserEntitlementTarget.FreeReviewOrder);

            await Assert.ThrowsAsync<ReviewOrderException>(() =>
                app.RunScopeAsync(services =>
                    services.GetRequiredService<ReviewOrderService>().CreateWithToken(new CreateTokenOrderCommand
                    {
                        UserNickname = "Nick-OtherOwner",
                        TrackUrl = "https://example.com/other-owner",
                        TrackDurationSeconds = 60,
                        UserComment = null,
                        UserEntitlementId = tokenId,
                        CreatedByUserId = requester.Id,
                    })));
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
                    UserNickname = "Nick-Partial",
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
        /// Проверяет, что donation-заказ нельзя создать без платежа.
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
                        UserNickname = "Nick-NoCoverage",
                        TrackUrl = "https://example.com/no-coverage",
                        TrackDurationSeconds = 60,
                        UserComment = null,
                        CreatedByUserId = user.Id,
                    })));
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
                    UserNickname = "Nick-Charity",
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
                        UserNickname = "Nick-Charity",
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
                        UserNickname = "Nick-NoStream",
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
                        UserNickname = "Nick-NoStream",
                        TrackUrl = "https://example.com/free",
                        TrackDurationSeconds = 60,
                        UserComment = null,
                        CreatedByUserId = user.Id,
                    })),
                "OutOfQueue" => app.RunScopeAsync(services =>
                    services.GetRequiredService<ReviewOrderService>().CreateOutOfQueue(new CreateOutOfQueueOrderCommand
                    {
                        UserNickname = "Nick-NoStream",
                        TrackUrl = null,
                        TrackDurationSeconds = null,
                        UserComment = null,
                        CreatedByUserId = user.Id,
                    })),
                _ => throw new InvalidOperationException($"Неподдерживаемый тип: {kind}")
            };

            await Assert.ThrowsAsync<ReviewOrderException>(() => action);
        }

        private static Task<long> CreateServiceToken(
            ApplicationTestHost app,
            UserEntity owner,
            string nickname,
            UserEntitlementTarget target) =>
            app.RunScopeAsync(async services =>
            {
                UnitOfWork uow = services.GetRequiredService<UnitOfWork>();
                UserEntity actualUser = await services.GetRequiredService<UserManager<UserEntity>>()
                    .FindByIdAsync(owner.Id.ToString())
                    ?? throw new InvalidOperationException("Пользователь не найден");
                UserNicknameEntity userNickname = await uow.UserNicknameStore.FindByNickname(nickname)
                    ?? uow.UserNicknameStore.Create(nickname);
                userNickname.UserId = owner.Id;

                UserEntitlementEntity token = uow.UserEntitlementStore.Create(
                    userNickname,
                    target,
                    actualUser);

                await uow.SaveChanges();

                return token.Id;
            });

        private static Task<DateTime?> GetTokenRedeemedAt(ApplicationTestHost app, long tokenId) =>
            app.RunScopeAsync(async services =>
            {
                IDbContextFactory<AppDbContext> factory = services.GetRequiredService<IDbContextFactory<AppDbContext>>();
                await using AppDbContext context = await factory.CreateDbContextAsync();
                return await context.UserEntitlements
                    .AsNoTracking()
                    .Where(x => x.Id == tokenId)
                    .Select(x => x.RedeemedAt)
                    .SingleAsync();
            });

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
