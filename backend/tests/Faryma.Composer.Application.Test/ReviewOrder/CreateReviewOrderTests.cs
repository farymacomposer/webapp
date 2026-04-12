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
        [Fact]
        public async Task CreateDonation_CreatesPendingOrderAndPayment_WhenTrackUrlProvided()
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("admin");
            ComposerStreamEntity stream = await app.Data.CreateStreamAsync(
                createdByUserId: user.Id,
                eventDate: app.Today.AddDays(1),
                type: ComposerStreamType.Donation);

            int expectedUpdates = app.QueueUpdateCount + 1;
            ReviewOrderEntity order = await app.RunScopeAsync(services =>
                services.GetRequiredService<ReviewOrderService>().CreateDonation(new CreateDonationOrderCommand
                {
                    Nickname = "Nick-Donation",
                    TrackUrl = "https://example.com/track",
                    UserComment = "comment",
                    PaymentAmount = 1_200,
                    TopUpProvider = AccountTopUpProvider.Manual,
                    CreatedByUserId = user.Id,
                }, CancellationToken.None));

            await app.WaitForQueueUpdateCountAsync(expectedUpdates);

            ReviewOrderEntity persisted = await app.GetOrderAsync(order.Id);
            List<TransactionEntity> orderTransactions = await app.GetOrderTransactionsAsync(order.Id);
            int transactionCount = await app.RunScopeAsync(async services =>
            {
                IDbContextFactory<AppDbContext> factory = services.GetRequiredService<IDbContextFactory<AppDbContext>>();
                await using AppDbContext context = await factory.CreateDbContextAsync();
                return await context.Transactions.CountAsync();
            });

            Assert.Equal(ReviewOrderType.Donation, persisted.Type);
            Assert.Equal(ReviewOrderStatus.Pending, persisted.Status);
            Assert.Equal(stream.Id, persisted.CreationStreamId);
            Assert.Equal("https://example.com/track", persisted.TrackUrl);
            Assert.Single(orderTransactions);
            Assert.Equal(TransactionKind.Payment, orderTransactions[0].Kind);
            Assert.Equal(1_200, orderTransactions[0].Debit);
            Assert.Equal(2, transactionCount);
        }

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

            int expectedUpdates = app.QueueUpdateCount + 1;
            ReviewOrderEntity order = await app.RunScopeAsync(services =>
                services.GetRequiredService<ReviewOrderService>().CreateFree(new CreateFreeOrderCommand
                {
                    Nickname = "Nick-Free",
                    TrackUrl = "https://example.com/free",
                    UserComment = null,
                    CreatedByUserId = user.Id,
                }, CancellationToken.None));

            await app.WaitForQueueUpdateCountAsync(expectedUpdates);

            Assert.NotEqual(nearerCharity.Id, order.CreationStreamId);
            Assert.Equal(donationStream.Id, order.CreationStreamId);
            Assert.Equal(ReviewOrderType.Free, order.Type);
        }

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

            int expectedUpdates = app.QueueUpdateCount + 1;
            ReviewOrderEntity order = await app.RunScopeAsync(services =>
                services.GetRequiredService<ReviewOrderService>().CreateOutOfQueue(new CreateOutOfQueueOrderCommand
                {
                    Nickname = "Nick-OOQ",
                    TrackUrl = null,
                    UserComment = null,
                    CreatedByUserId = user.Id,
                }, CancellationToken.None));

            await app.WaitForQueueUpdateCountAsync(expectedUpdates);

            Assert.Equal(nearest.Id, order.CreationStreamId);
            Assert.Equal(ReviewOrderType.OutOfQueue, order.Type);
            Assert.Equal(ReviewOrderStatus.Preorder, order.Status);
            Assert.Equal(0, order.PayableAmount);
        }

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

            int expectedUpdates = app.QueueUpdateCount + 1;
            ReviewOrderEntity order = await app.RunScopeAsync(services =>
                services.GetRequiredService<ReviewOrderService>().CreateCharity(new CreateCharityOrderCommand
                {
                    Nickname = "Nick-Charity",
                    TrackUrl = "https://example.com/charity",
                    UserComment = null,
                    CreatedByUserId = user.Id,
                }, CancellationToken.None));

            await app.WaitForQueueUpdateCountAsync(expectedUpdates);

            Assert.Equal(charityStream.Id, order.CreationStreamId);
            Assert.Equal(ReviewOrderType.Charity, order.Type);
            Assert.Equal(0, order.PayableAmount);
        }

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

            ReviewOrderException exception = await Assert.ThrowsAsync<ReviewOrderException>(() =>
                app.RunScopeAsync(services =>
                    services.GetRequiredService<ReviewOrderService>().CreateCharity(new CreateCharityOrderCommand
                    {
                        Nickname = "Nick-Charity",
                        TrackUrl = "https://example.com/charity",
                        UserComment = null,
                        CreatedByUserId = user.Id,
                    }, CancellationToken.None)));

            Assert.Equal("Не запущен благотворительный стрим.", exception.Message);
        }
    }
}