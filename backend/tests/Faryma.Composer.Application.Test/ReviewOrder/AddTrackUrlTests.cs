using Faryma.Composer.Application.Features.ReviewOrder;
using Faryma.Composer.Application.Test.Infrastructure;
using Faryma.Composer.Contracts.Application.Features.ReviewOrder.Commands;
using Faryma.Composer.Contracts.Infrastructure.Entities;
using Faryma.Composer.Contracts.Infrastructure.Entities.TransactionSources;
using Faryma.Composer.Contracts.Infrastructure.Enums;

namespace Faryma.Composer.Application.Test.ReviewOrder
{
    public sealed class AddTrackUrlTests(PostgreSqlFixture fixture) : ApplicationTestBase(fixture)
    {
        [Fact]
        public async Task AddTrackUrl_MovesPreorderToPending()
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("admin");
            ReviewOrderEntity order = await app.Data.CreateReviewOrderAsync(
                createdByUserId: user.Id,
                status: ReviewOrderStatus.Preorder,
                trackUrl: null);

            ReviewOrderEntity result = await app.RunScopeAsync(services =>
                services.GetRequiredService<ReviewOrderService>().AddTrackUrl(new AddTrackUrlCommand
                {
                    ReviewOrderId = order.Id,
                    TrackUrl = "https://example.com/new-track",
                }, CancellationToken.None));

            Assert.Equal(ReviewOrderStatus.Pending, result.Status);
            Assert.Equal("https://example.com/new-track", result.TrackUrl);
        }

        [Theory]
        [InlineData(ReviewOrderStatus.Pending)]
        [InlineData(ReviewOrderStatus.InProgress)]
        public async Task AddTrackUrl_UpdatesUrlWithoutChangingStatus(ReviewOrderStatus status)
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("admin");
            ReviewOrderEntity order = await app.Data.CreateReviewOrderAsync(
                createdByUserId: user.Id,
                status: status,
                inProgressAt: status == ReviewOrderStatus.InProgress ? app.FixedNow : null);

            ReviewOrderEntity result = await app.RunScopeAsync(services =>
                services.GetRequiredService<ReviewOrderService>().AddTrackUrl(new AddTrackUrlCommand
                {
                    ReviewOrderId = order.Id,
                    TrackUrl = "https://example.com/updated-track",
                }, CancellationToken.None));

            Assert.Equal(status, result.Status);
            Assert.Equal("https://example.com/updated-track", result.TrackUrl);
        }

        [Theory]
        [InlineData(ReviewOrderStatus.Completed)]
        [InlineData(ReviewOrderStatus.Canceled)]
        public async Task AddTrackUrl_Throws_WhenOrderHasInvalidStatus(ReviewOrderStatus status)
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("admin");
            ReviewOrderEntity order = await app.Data.CreateReviewOrderAsync(
                createdByUserId: user.Id,
                status: status,
                completedAt: status == ReviewOrderStatus.Completed ? app.FixedNow : null,
                canceledAt: status == ReviewOrderStatus.Canceled ? app.FixedNow : null,
                cancelReason: status == ReviewOrderStatus.Canceled ? "reason" : null,
                reviewRating: status == ReviewOrderStatus.Completed ? 9 : null);

            await Assert.ThrowsAsync<ReviewOrderException>(() =>
                app.RunScopeAsync(services =>
                    services.GetRequiredService<ReviewOrderService>().AddTrackUrl(new AddTrackUrlCommand
                    {
                        ReviewOrderId = order.Id,
                        TrackUrl = "https://example.com/fail-track",
                    }, CancellationToken.None)));
        }
    }
}