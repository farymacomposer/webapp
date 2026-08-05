using Faryma.Composer.Application.Features.ReviewOrder;
using Faryma.Composer.Application.Features.ReviewOrder.CreateDonation;
using Faryma.Composer.Application.Test.Infrastructure;
using Faryma.Composer.Domain.Entities;
using Faryma.Composer.Domain.Entities.TransactionSources;
using Faryma.Composer.Domain.Enums;
using Faryma.Composer.Domain.Exceptions;

namespace Faryma.Composer.Application.Test.ReviewOrder
{
    public sealed class TakeOrderInProgressTests(PostgreSqlFixture fixture) : TestBase(fixture)
    {
        /// <summary>
        /// Проверяет, что pending-заказ переводится в статус InProgress.
        /// </summary>
        [Fact]
        public async Task TakeInProgress_TransitionsPendingOrderToInProgress()
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("admin");
            ComposerStreamEntity liveStream = await app.Data.CreateStreamAsync(
                createdByUserId: user.Id,
                type: ComposerStreamType.Donation,
                status: ComposerStreamStatus.Live,
                startedAt: app.FixedNow);

            ReviewOrderEntity order = await app.RunScopeAsync(services =>
                services.GetRequiredService<ReviewOrderService>().CreateDonation(new CreateDonationCommand
                {
                    UserNickname = "Nick-Take",
                    TrackUrl = "https://example.com/take",
                    TrackDurationSeconds = 60,
                    UserComment = null,
                    PaymentAmount = 1_000,
                    TopUpProvider = AccountTopUpProvider.Manual,
                    CreatedByUserId = user.Id,
                }));
            await app.DrainQueueEventsAsync();

            ReviewOrderEntity result = await app.RunScopeAsync(services =>
                services.GetRequiredService<ReviewOrderService>().TakeInProgress(order.Id));

            ReviewOrderEntity persisted = await app.GetOrderAsync(order.Id);
            Assert.Equal(ReviewOrderStatus.InProgress, result.Status);
            Assert.Equal(ReviewOrderStatus.InProgress, persisted.Status);
            Assert.Equal(liveStream.Id, persisted.ProcessingStreamId);
            Assert.Equal(app.FixedNow, persisted.InProgressAt);
            Assert.NotEqual(QueueCategory.Unspecified, persisted.QueueCategory);
        }

        /// <summary>
        /// Проверяет, что повторный перевод уже взятого заказа ничего не меняет.
        /// </summary>
        [Fact]
        public async Task TakeInProgress_ReturnsCurrentOrder_WhenOrderAlreadyInProgress()
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("admin");
            ComposerStreamEntity liveStream = await app.Data.CreateStreamAsync(
                createdByUserId: user.Id,
                type: ComposerStreamType.Donation,
                status: ComposerStreamStatus.Live,
                startedAt: app.FixedNow);
            DateTime originalInProgressAt = app.FixedNow.AddMinutes(-5);
            ReviewOrderEntity order = await app.Data.CreateReviewOrderAsync(
                createdByUserId: user.Id,
                creationStreamId: liveStream.Id,
                processingStreamId: liveStream.Id,
                nickname: "Nick-InProgress",
                status: ReviewOrderStatus.InProgress,
                totalPaymentAmount: 1_000,
                inProgressAt: originalInProgressAt);

            int beforeUpdates = app.QueueUpdateCount;
            ReviewOrderEntity result = await app.RunScopeAsync(services =>
                services.GetRequiredService<ReviewOrderService>().TakeInProgress(order.Id));
            ReviewOrderEntity persisted = await app.GetOrderAsync(order.Id);

            Assert.Equal(order.Id, result.Id);
            Assert.Equal(originalInProgressAt, result.InProgressAt);
            Assert.Equal(originalInProgressAt, persisted.InProgressAt);
            Assert.Equal(ReviewOrderStatus.InProgress, persisted.Status);
            Assert.Equal(liveStream.Id, persisted.ProcessingStreamId);
            Assert.Equal(beforeUpdates, app.QueueUpdateCount);
        }

        /// <summary>
        /// Проверяет, что замороженный заказ нельзя взять в работу.
        /// </summary>
        [Fact]
        public async Task TakeInProgress_Throws_WhenOrderIsFrozen()
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("admin");
            ComposerStreamEntity liveStream = await app.Data.CreateStreamAsync(
                createdByUserId: user.Id,
                status: ComposerStreamStatus.Live,
                startedAt: app.FixedNow);
            ReviewOrderEntity order = await app.Data.CreateReviewOrderAsync(
                createdByUserId: user.Id,
                creationStreamId: liveStream.Id,
                status: ReviewOrderStatus.Pending,
                isFrozen: true);

            await Assert.ThrowsAsync<ReviewOrderException>(() =>
                app.RunScopeAsync(services =>
                    services.GetRequiredService<ReviewOrderService>().TakeInProgress(order.Id)));
        }

        /// <summary>
        /// Проверяет, что без активного стрима заказ нельзя взять в работу.
        /// </summary>
        [Fact]
        public async Task TakeInProgress_Throws_WhenLiveStreamIsMissing()
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("admin");
            ComposerStreamEntity plannedStream = await app.Data.CreateStreamAsync(
                createdByUserId: user.Id,
                status: ComposerStreamStatus.Planned);
            ReviewOrderEntity order = await app.Data.CreateReviewOrderAsync(
                createdByUserId: user.Id,
                creationStreamId: plannedStream.Id,
                status: ReviewOrderStatus.Pending);

            await Assert.ThrowsAsync<ReviewOrderException>(() =>
                app.RunScopeAsync(services =>
                    services.GetRequiredService<ReviewOrderService>().TakeInProgress(order.Id)));
        }

        /// <summary>
        /// Проверяет, что нельзя взять новый заказ, пока другой уже находится в работе.
        /// </summary>
        [Fact]
        public async Task TakeInProgress_Throws_WhenAnotherOrderIsAlreadyInProgress()
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("admin");
            ComposerStreamEntity liveStream = await app.Data.CreateStreamAsync(
                createdByUserId: user.Id,
                status: ComposerStreamStatus.Live,
                startedAt: app.FixedNow);

            await app.Data.CreateReviewOrderAsync(
                createdByUserId: user.Id,
                creationStreamId: liveStream.Id,
                processingStreamId: liveStream.Id,
                nickname: "Nick-Existing",
                status: ReviewOrderStatus.InProgress,
                inProgressAt: app.FixedNow);

            ReviewOrderEntity candidate = await app.Data.CreateReviewOrderAsync(
                createdByUserId: user.Id,
                creationStreamId: liveStream.Id,
                nickname: "Nick-Candidate",
                status: ReviewOrderStatus.Pending);

            await Assert.ThrowsAsync<ReviewOrderException>(() =>
                app.RunScopeAsync(services =>
                    services.GetRequiredService<ReviewOrderService>().TakeInProgress(candidate.Id)));
        }

        /// <summary>
        /// Проверяет, что предзаказ нельзя перевести в работу без ссылки.
        /// </summary>
        [Fact]
        public async Task TakeInProgress_Throws_WhenOrderIsPreorder()
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("admin");
            ComposerStreamEntity liveStream = await app.Data.CreateStreamAsync(
                createdByUserId: user.Id,
                status: ComposerStreamStatus.Live,
                startedAt: app.FixedNow);
            ReviewOrderEntity order = await app.Data.CreateReviewOrderAsync(
                createdByUserId: user.Id,
                creationStreamId: liveStream.Id,
                status: ReviewOrderStatus.Preorder,
                trackUrl: null);

            await Assert.ThrowsAsync<ReviewOrderException>(() =>
                app.RunScopeAsync(services =>
                    services.GetRequiredService<ReviewOrderService>().TakeInProgress(order.Id)));
        }

        /// <summary>
        /// Проверяет, что в работу можно взять только заказ в статусе Pending.
        /// </summary>
        [Theory]
        [InlineData(ReviewOrderStatus.AwaitingPayment)]
        [InlineData(ReviewOrderStatus.Completed)]
        [InlineData(ReviewOrderStatus.Canceled)]
        public async Task TakeInProgress_Throws_WhenOrderIsNotPending(ReviewOrderStatus status)
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("admin");
            ComposerStreamEntity liveStream = await app.Data.CreateStreamAsync(
                createdByUserId: user.Id,
                status: ComposerStreamStatus.Live,
                startedAt: app.FixedNow);
            ReviewOrderEntity order = await app.Data.CreateReviewOrderAsync(
                createdByUserId: user.Id,
                creationStreamId: liveStream.Id,
                status: status,
                completedAt: status == ReviewOrderStatus.Completed ? app.FixedNow : null,
                canceledAt: status == ReviewOrderStatus.Canceled ? app.FixedNow : null,
                cancelReason: status == ReviewOrderStatus.Canceled ? "reason" : null,
                reviewRating: status == ReviewOrderStatus.Completed ? 10 : null);

            await Assert.ThrowsAsync<ReviewOrderException>(() =>
                app.RunScopeAsync(services =>
                    services.GetRequiredService<ReviewOrderService>().TakeInProgress(order.Id)));
        }

        /// <summary>
        /// Проверяет, что для несуществующего заказа выбрасывается ошибка.
        /// </summary>
        [Fact]
        public async Task TakeInProgress_Throws_WhenOrderDoesNotExist()
        {
            await using ApplicationTestHost app = await CreateAppAsync();

            await Assert.ThrowsAsync<ReviewOrderException>(() =>
                app.RunScopeAsync(services =>
                    services.GetRequiredService<ReviewOrderService>().TakeInProgress(long.MaxValue)));
        }
    }
}
