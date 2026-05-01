using Faryma.Composer.Application.Features.ReviewOrder;
using Faryma.Composer.Application.Test.Infrastructure;
using Faryma.Composer.Contracts.Infrastructure.Entities;
using Faryma.Composer.Contracts.Infrastructure.Entities.TransactionSources;
using Faryma.Composer.Contracts.Infrastructure.Enums;

namespace Faryma.Composer.Application.Test.ReviewOrder
{
    public sealed class UnfreezeReviewOrderTests(PostgreSqlFixture fixture) : TestBase(fixture)
    {
        /// <summary>
        /// Проверяет, что для допустимых статусов снимается флаг заморозки.
        /// </summary>
        [Theory]
        [InlineData(ReviewOrderStatus.Preorder)]
        [InlineData(ReviewOrderStatus.Pending)]
        public async Task Unfreeze_ClearsFrozenFlag_ForAllowedStatuses(ReviewOrderStatus status)
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("admin");
            ReviewOrderEntity order = await app.Data.CreateReviewOrderAsync(
                createdByUserId: user.Id,
                status: status,
                isFrozen: true,
                trackUrl: status == ReviewOrderStatus.Preorder ? null : "https://example.com/track");

            ReviewOrderEntity result = await app.RunScopeAsync(services =>
                services.GetRequiredService<ReviewOrderService>().Unfreeze(order.Id));
            ReviewOrderEntity persisted = await app.GetOrderAsync(order.Id);

            Assert.False(result.IsFrozen);
            Assert.False(persisted.IsFrozen);
        }

        /// <summary>
        /// Проверяет, что повторная разморозка не меняет уже размороженный заказ.
        /// </summary>
        [Fact]
        public async Task Unfreeze_ReturnsCurrentOrder_WhenAlreadyUnfrozen()
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("admin");
            ReviewOrderEntity order = await app.Data.CreateReviewOrderAsync(
                createdByUserId: user.Id,
                status: ReviewOrderStatus.Pending,
                isFrozen: false);

            ReviewOrderEntity result = await app.RunScopeAsync(services =>
                services.GetRequiredService<ReviewOrderService>().Unfreeze(order.Id));
            ReviewOrderEntity persisted = await app.GetOrderAsync(order.Id);

            Assert.False(result.IsFrozen);
            Assert.False(persisted.IsFrozen);
        }

        /// <summary>
        /// Проверяет, что разморозка запрещена для недопустимых статусов.
        /// </summary>
        [Theory]
        [InlineData(ReviewOrderStatus.InProgress)]
        [InlineData(ReviewOrderStatus.Completed)]
        [InlineData(ReviewOrderStatus.Canceled)]
        public async Task Unfreeze_Throws_WhenStatusIsInvalid(ReviewOrderStatus status)
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("admin");
            ReviewOrderEntity order = await app.Data.CreateReviewOrderAsync(
                createdByUserId: user.Id,
                status: status,
                isFrozen: true,
                inProgressAt: status == ReviewOrderStatus.InProgress ? app.FixedNow : null,
                completedAt: status == ReviewOrderStatus.Completed ? app.FixedNow : null,
                canceledAt: status == ReviewOrderStatus.Canceled ? app.FixedNow : null,
                cancelReason: status == ReviewOrderStatus.Canceled ? "reason" : null,
                reviewRating: status == ReviewOrderStatus.Completed ? 10 : null);

            await Assert.ThrowsAsync<ReviewOrderException>(() =>
                app.RunScopeAsync(services =>
                    services.GetRequiredService<ReviewOrderService>().Unfreeze(order.Id)));
        }

        /// <summary>
        /// Проверяет, что для несуществующего заказа выбрасывается ошибка.
        /// </summary>
        [Fact]
        public async Task Unfreeze_Throws_WhenOrderDoesNotExist()
        {
            await using ApplicationTestHost app = await CreateAppAsync();

            await Assert.ThrowsAsync<ReviewOrderException>(() =>
                app.RunScopeAsync(services =>
                    services.GetRequiredService<ReviewOrderService>().Unfreeze(long.MaxValue)));
        }
    }
}
