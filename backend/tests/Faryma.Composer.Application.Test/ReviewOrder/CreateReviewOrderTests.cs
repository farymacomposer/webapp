using Faryma.Composer.Application.Features.ReviewOrder;
using Faryma.Composer.Application.Test.Infrastructure;
using Faryma.Composer.Contracts.Application.Features.ReviewOrder.Commands;
using Faryma.Composer.Contracts.Infrastructure.Entities;
using Faryma.Composer.Contracts.Infrastructure.Entities.TransactionSources;
using Faryma.Composer.Contracts.Infrastructure.Enums;

namespace Faryma.Composer.Application.Test.ReviewOrder
{
    public sealed class CreateReviewOrderTests(PostgreSqlFixture fixture) : ApplicationTestBase(fixture)
    {
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
                    UserComment = "comment",
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
                    UserComment = null,
                    PaymentAmount = 800,
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
                    UserComment = null,
                    CreatedByUserId = user.Id,
                }));

            Assert.NotEqual(nearerCharity.Id, order.CreationStreamId);
            Assert.Equal(donationStream.Id, order.CreationStreamId);
            Assert.Equal(ReviewOrderType.Free, order.Type);
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
                    Nickname = "Nick-OOQ",
                    TrackUrl = null,
                    UserComment = null,
                    CreatedByUserId = user.Id,
                }));

            Assert.Equal(nearest.Id, order.CreationStreamId);
            Assert.Equal(ReviewOrderType.OutOfQueue, order.Type);
            Assert.Equal(ReviewOrderStatus.Preorder, order.Status);
            Assert.Equal(0, order.PayableAmount);
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
                        UserComment = null,
                        CreatedByUserId = user.Id,
                    })),
                "OutOfQueue" => app.RunScopeAsync(services =>
                    services.GetRequiredService<ReviewOrderService>().CreateOutOfQueue(new CreateOutOfQueueOrderCommand
                    {
                        Nickname = "Nick-NoStream",
                        TrackUrl = null,
                        UserComment = null,
                        CreatedByUserId = user.Id,
                    })),
                _ => throw new InvalidOperationException($"Unsupported kind: {kind}")
            };

            await Assert.ThrowsAsync<ReviewOrderException>(() => action);
        }
    }
}