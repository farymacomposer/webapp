using Faryma.Composer.Application.Features.ReviewOrder;
using Faryma.Composer.Application.Test.Infrastructure;
using Faryma.Composer.Contracts.Application.Features.ReviewOrder.Commands;
using Faryma.Composer.Contracts.Infrastructure.Entities;
using Faryma.Composer.Contracts.Infrastructure.Entities.TransactionSources;
using Faryma.Composer.Contracts.Infrastructure.Enums;

namespace Faryma.Composer.Application.Test.ReviewOrder
{
    public sealed class CancelReviewOrderTests(PostgreSqlFixture fixture) : ApplicationTestBase(fixture)
    {
        [Fact]
        public async Task Cancel_ClearsProcessingFields_WhenOrderWasInProgress()
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
                processingStreamId: liveStream.Id,
                status: ReviewOrderStatus.InProgress,
                queueCategory: QueueCategory.Donation,
                inProgressAt: app.FixedNow);

            ReviewOrderEntity result = await app.RunScopeAsync(services =>
                services.GetRequiredService<ReviewOrderService>().Cancel(new CancelCommand
                {
                    ReviewOrderId = order.Id,
                    CancelReason = "duplicate",
                }, CancellationToken.None));

            ReviewOrderEntity persisted = await app.GetOrderAsync(order.Id);
            Assert.Equal(ReviewOrderStatus.Canceled, result.Status);
            Assert.Equal(ReviewOrderStatus.Canceled, persisted.Status);
            Assert.Equal("duplicate", persisted.CancelReason);
            Assert.Equal(app.FixedNow, persisted.CanceledAt);
            Assert.Null(persisted.ProcessingStreamId);
            Assert.Null(persisted.InProgressAt);
            Assert.Equal(QueueCategory.Unspecified, persisted.QueueCategory);
        }

        [Fact]
        public async Task Cancel_ReturnsCurrentOrder_WhenOrderAlreadyCanceled()
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("admin");
            ReviewOrderEntity order = await app.Data.CreateReviewOrderAsync(
                createdByUserId: user.Id,
                status: ReviewOrderStatus.Canceled,
                canceledAt: app.FixedNow,
                cancelReason: "reason");

            int beforeUpdates = app.QueueUpdateCount;
            ReviewOrderEntity result = await app.RunScopeAsync(services =>
                services.GetRequiredService<ReviewOrderService>().Cancel(new CancelCommand
                {
                    ReviewOrderId = order.Id,
                    CancelReason = "another",
                }, CancellationToken.None));

            Assert.Equal(order.Id, result.Id);
            Assert.Equal(beforeUpdates, app.QueueUpdateCount);
        }

        [Fact]
        public async Task Cancel_Throws_WhenOrderIsCompleted()
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("admin");
            ReviewOrderEntity order = await app.Data.CreateReviewOrderAsync(
                createdByUserId: user.Id,
                status: ReviewOrderStatus.Completed,
                completedAt: app.FixedNow,
                reviewRating: 10);

            await Assert.ThrowsAsync<ReviewOrderException>(() =>
                app.RunScopeAsync(services =>
                    services.GetRequiredService<ReviewOrderService>().Cancel(new CancelCommand
                    {
                        ReviewOrderId = order.Id,
                        CancelReason = "late",
                    }, CancellationToken.None)));
        }
    }
}