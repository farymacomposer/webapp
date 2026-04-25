using Faryma.Composer.Application.Features.ReviewOrder;
using Faryma.Composer.Application.Test.Infrastructure;
using Faryma.Composer.Contracts.Infrastructure.Entities;
using Faryma.Composer.Contracts.Infrastructure.Entities.TransactionSources;
using Faryma.Composer.Contracts.Infrastructure.Enums;

namespace Faryma.Composer.Application.Test.ReviewOrder
{
    public sealed class FreezeReviewOrderTests(PostgreSqlFixture fixture) : ApplicationTestBase(fixture)
    {
        /// <summary>
        /// Проверяет, что для допустимых статусов выставляется флаг заморозки.
        /// </summary>
        [Theory]
        [InlineData(ReviewOrderStatus.Preorder)]
        [InlineData(ReviewOrderStatus.Pending)]
        public async Task Freeze_SetsFrozenFlag_ForAllowedStatuses(ReviewOrderStatus status)
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("admin");
            ReviewOrderEntity order = await app.Data.CreateReviewOrderAsync(
                createdByUserId: user.Id,
                status: status,
                trackUrl: status == ReviewOrderStatus.Preorder ? null : "https://example.com/track");

            ReviewOrderEntity result = await app.RunScopeAsync(services =>
                services.GetRequiredService<ReviewOrderService>().Freeze(order.Id));
            ReviewOrderEntity persisted = await app.GetOrderAsync(order.Id);

            Assert.True(result.IsFrozen);
            Assert.True(persisted.IsFrozen);
        }

        /// <summary>
        /// Проверяет, что повторная заморозка не меняет уже замороженный заказ.
        /// </summary>
        [Fact]
        public async Task Freeze_ReturnsCurrentOrder_WhenAlreadyFrozen()
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("admin");
            ReviewOrderEntity order = await app.Data.CreateReviewOrderAsync(
                createdByUserId: user.Id,
                status: ReviewOrderStatus.Pending,
                isFrozen: true);

            ReviewOrderEntity result = await app.RunScopeAsync(services =>
                services.GetRequiredService<ReviewOrderService>().Freeze(order.Id));
            ReviewOrderEntity persisted = await app.GetOrderAsync(order.Id);

            Assert.True(result.IsFrozen);
            Assert.True(persisted.IsFrozen);
        }

        /// <summary>
        /// Проверяет, что заморозка запрещена для недопустимых статусов.
        /// </summary>
        [Theory]
        [InlineData(ReviewOrderStatus.InProgress)]
        [InlineData(ReviewOrderStatus.Completed)]
        [InlineData(ReviewOrderStatus.Canceled)]
        public async Task Freeze_Throws_WhenStatusIsInvalid(ReviewOrderStatus status)
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("admin");
            ReviewOrderEntity order = await app.Data.CreateReviewOrderAsync(
                createdByUserId: user.Id,
                status: status,
                inProgressAt: status == ReviewOrderStatus.InProgress ? app.FixedNow : null,
                completedAt: status == ReviewOrderStatus.Completed ? app.FixedNow : null,
                canceledAt: status == ReviewOrderStatus.Canceled ? app.FixedNow : null,
                cancelReason: status == ReviewOrderStatus.Canceled ? "reason" : null,
                reviewRating: status == ReviewOrderStatus.Completed ? 10 : null);

            await Assert.ThrowsAsync<ReviewOrderException>(() =>
                app.RunScopeAsync(services =>
                    services.GetRequiredService<ReviewOrderService>().Freeze(order.Id)));
        }

        /// <summary>
        /// Проверяет, что для несуществующего заказа выбрасывается ошибка.
        /// </summary>
        [Fact]
        public async Task Freeze_Throws_WhenOrderDoesNotExist()
        {
            await using ApplicationTestHost app = await CreateAppAsync();

            await Assert.ThrowsAsync<ReviewOrderException>(() =>
                app.RunScopeAsync(services =>
                    services.GetRequiredService<ReviewOrderService>().Freeze(long.MaxValue)));
        }
    }
}