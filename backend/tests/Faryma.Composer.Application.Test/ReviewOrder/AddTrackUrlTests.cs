using Faryma.Composer.Application.Features.ReviewOrder;
using Faryma.Composer.Application.Test.Infrastructure;
using Faryma.Composer.Contracts.Application.Features.ReviewOrder.Commands;
using Faryma.Composer.Contracts.Infrastructure.Entities;
using Faryma.Composer.Contracts.Infrastructure.Entities.TransactionSources;
using Faryma.Composer.Contracts.Infrastructure.Enums;

namespace Faryma.Composer.Application.Test.ReviewOrder
{
    public sealed class AddTrackUrlTests(PostgreSqlFixture fixture) : TestBase(fixture)
    {
        /// <summary>
        /// Проверяет, что добавление ссылки переводит предзаказ в статус Pending.
        /// </summary>
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
                }));
            ReviewOrderEntity persisted = await app.GetOrderAsync(order.Id);

            Assert.Equal(ReviewOrderStatus.Pending, result.Status);
            Assert.Equal("https://example.com/new-track", result.TrackUrl);
            Assert.Equal(ReviewOrderStatus.Pending, persisted.Status);
            Assert.Equal("https://example.com/new-track", persisted.TrackUrl);
        }

        /// <summary>
        /// Проверяет, что ссылка обновляется без смены допустимого статуса заказа.
        /// </summary>
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
                }));
            ReviewOrderEntity persisted = await app.GetOrderAsync(order.Id);

            Assert.Equal(status, result.Status);
            Assert.Equal("https://example.com/updated-track", result.TrackUrl);
            Assert.Equal(status, persisted.Status);
            Assert.Equal("https://example.com/updated-track", persisted.TrackUrl);
        }

        /// <summary>
        /// Проверяет, что у завершенного или отмененного заказа ссылку менять нельзя.
        /// </summary>
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
                    })));
        }

        /// <summary>
        /// Проверяет, что для несуществующего заказа выбрасывается ошибка.
        /// </summary>
        [Fact]
        public async Task AddTrackUrl_Throws_WhenOrderDoesNotExist()
        {
            await using ApplicationTestHost app = await CreateAppAsync();

            await Assert.ThrowsAsync<ReviewOrderException>(() =>
                app.RunScopeAsync(services =>
                    services.GetRequiredService<ReviewOrderService>().AddTrackUrl(new AddTrackUrlCommand
                    {
                        ReviewOrderId = long.MaxValue,
                        TrackUrl = "https://example.com/missing-track",
                    })));
        }
    }
}
