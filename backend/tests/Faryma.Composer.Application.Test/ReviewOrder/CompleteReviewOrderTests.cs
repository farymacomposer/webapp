using Faryma.Composer.Application.Features.ReviewOrder;
using Faryma.Composer.Application.Test.Infrastructure;
using Faryma.Composer.Contracts.Application.Features.ReviewOrder.Commands;
using Faryma.Composer.Contracts.Infrastructure.Entities;
using Faryma.Composer.Contracts.Infrastructure.Entities.TransactionSources;
using Faryma.Composer.Contracts.Infrastructure.Enums;

namespace Faryma.Composer.Application.Test.ReviewOrder
{
    public sealed class CompleteReviewOrderTests(PostgreSqlFixture fixture) : ApplicationTestBase(fixture)
    {
        [Fact]
        public async Task Complete_CreatesReviewAndMarksOrderCompleted()
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
                inProgressAt: app.FixedNow);

            ReviewOrderEntity result = await app.RunScopeAsync(services =>
                services.GetRequiredService<ReviewOrderService>().Complete(new CompleteCommand
                {
                    ReviewOrderId = order.Id,
                    Rating = 26,
                    CreatedByUserId = user.Id,
                }, CancellationToken.None));

            ReviewOrderEntity persisted = await app.GetOrderAsync(order.Id);
            Assert.Equal(ReviewOrderStatus.Completed, result.Status);
            Assert.Equal(ReviewOrderStatus.Completed, persisted.Status);
            Assert.Equal(app.FixedNow, persisted.CompletedAt);
            Assert.NotNull(persisted.Review);
            Assert.Equal(26, persisted.Review!.RatingValue);
            Assert.Equal(1, await app.GetReviewCountAsync());
        }

        [Fact]
        public async Task Complete_ReturnsCurrentOrder_WhenOrderAlreadyCompleted()
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("admin");
            ReviewOrderEntity order = await app.Data.CreateReviewOrderAsync(
                createdByUserId: user.Id,
                status: ReviewOrderStatus.Completed,
                completedAt: app.FixedNow,
                reviewRating: 12);

            ReviewOrderEntity result = await app.RunScopeAsync(services =>
                services.GetRequiredService<ReviewOrderService>().Complete(new CompleteCommand
                {
                    ReviewOrderId = order.Id,
                    Rating = 20,
                    CreatedByUserId = user.Id,
                }, CancellationToken.None));

            Assert.Equal(order.Id, result.Id);
            Assert.Equal(12, result.Review!.RatingValue);
            Assert.Equal(app.FixedNow, result.CompletedAt);
            Assert.Equal(1, await app.GetReviewCountAsync());
        }

        [Theory]
        [InlineData(ReviewOrderStatus.Preorder)]
        [InlineData(ReviewOrderStatus.Pending)]
        [InlineData(ReviewOrderStatus.Canceled)]
        public async Task Complete_Throws_WhenOrderIsNotInProgress(ReviewOrderStatus status)
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("admin");
            ReviewOrderEntity order = await app.Data.CreateReviewOrderAsync(
                createdByUserId: user.Id,
                status: status,
                canceledAt: status == ReviewOrderStatus.Canceled ? app.FixedNow : null,
                cancelReason: status == ReviewOrderStatus.Canceled ? "reason" : null);

            await Assert.ThrowsAsync<ReviewOrderException>(() =>
                app.RunScopeAsync(services =>
                    services.GetRequiredService<ReviewOrderService>().Complete(new CompleteCommand
                    {
                        ReviewOrderId = order.Id,
                        Rating = 15,
                        CreatedByUserId = user.Id,
                    }, CancellationToken.None)));
        }

        [Fact]
        public async Task Complete_Throws_WhenOrderDoesNotExist()
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("admin");

            await Assert.ThrowsAsync<ReviewOrderException>(() =>
                app.RunScopeAsync(services =>
                    services.GetRequiredService<ReviewOrderService>().Complete(new CompleteCommand
                    {
                        ReviewOrderId = long.MaxValue,
                        Rating = 15,
                        CreatedByUserId = user.Id,
                    }, CancellationToken.None)));
        }
    }
}