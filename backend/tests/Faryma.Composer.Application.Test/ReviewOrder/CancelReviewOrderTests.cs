using Faryma.Composer.Application.Features.ReviewOrder;
using Faryma.Composer.Application.Test.Infrastructure;
using Faryma.Composer.Contracts.Application.Features.ReviewOrder.Commands;
using Faryma.Composer.Contracts.Infrastructure.Entities;
using Faryma.Composer.Contracts.Infrastructure.Entities.TransactionSources;
using Faryma.Composer.Contracts.Infrastructure.Enums;

namespace Faryma.Composer.Application.Test.ReviewOrder
{
    public sealed class CancelReviewOrderTests(PostgreSqlFixture fixture) : TestBase(fixture)
    {
        /// <summary>
        /// Проверяет, что отмена заказа в работе очищает поля обработки.
        /// </summary>
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
                    CancelReason = "дубликат",
                }));

            ReviewOrderEntity persisted = await app.GetOrderAsync(order.Id);
            Assert.Equal(ReviewOrderStatus.Canceled, result.Status);
            Assert.Equal(ReviewOrderStatus.Canceled, persisted.Status);
            Assert.Equal("дубликат", persisted.CancelReason);
            Assert.Equal(app.FixedNow, persisted.CanceledAt);
            Assert.Null(persisted.ProcessingStreamId);
            Assert.Null(persisted.InProgressAt);
            Assert.Equal(QueueCategory.Unspecified, persisted.QueueCategory);
        }

        /// <summary>
        /// Проверяет, что повторная отмена возвращает уже сохраненное состояние заказа.
        /// </summary>
        [Fact]
        public async Task Cancel_ReturnsCurrentOrder_WhenOrderAlreadyCanceled()
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("admin");
            ReviewOrderEntity order = await app.Data.CreateReviewOrderAsync(
                createdByUserId: user.Id,
                status: ReviewOrderStatus.Canceled,
                canceledAt: app.FixedNow,
                cancelReason: "причина");

            ReviewOrderEntity result = await app.RunScopeAsync(services =>
                services.GetRequiredService<ReviewOrderService>().Cancel(new CancelCommand
                {
                    ReviewOrderId = order.Id,
                    CancelReason = "другая причина",
                }));

            ReviewOrderEntity persisted = await app.GetOrderAsync(order.Id);
            Assert.Equal(order.Id, result.Id);
            Assert.Equal("причина", result.CancelReason);
            Assert.Equal(app.FixedNow, result.CanceledAt);
            Assert.Equal("причина", persisted.CancelReason);
            Assert.Equal(app.FixedNow, persisted.CanceledAt);
        }

        /// <summary>
        /// Проверяет, что завершенный заказ отменить нельзя.
        /// </summary>
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
                        CancelReason = "поздняя отмена",
                    })));
        }

        /// <summary>
        /// Проверяет, что отмена допустимых неактивных статусов тоже очищает поля обработки.
        /// </summary>
        [Theory]
        [InlineData(ReviewOrderStatus.Preorder)]
        [InlineData(ReviewOrderStatus.Pending)]
        public async Task Cancel_ClearsProcessingFields_WhenOrderHasAllowedNonProcessingStatus(ReviewOrderStatus status)
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("admin");
            ReviewOrderEntity order = await app.Data.CreateReviewOrderAsync(
                createdByUserId: user.Id,
                status: status,
                trackUrl: status == ReviewOrderStatus.Preorder ? null : "https://example.com/track",
                queueCategory: QueueCategory.Donation,
                cancelReason: null);

            ReviewOrderEntity result = await app.RunScopeAsync(services =>
                services.GetRequiredService<ReviewOrderService>().Cancel(new CancelCommand
                {
                    ReviewOrderId = order.Id,
                    CancelReason = "ручная отмена",
                }));
            ReviewOrderEntity persisted = await app.GetOrderAsync(order.Id);

            Assert.Equal(ReviewOrderStatus.Canceled, result.Status);
            Assert.Equal("ручная отмена", result.CancelReason);
            Assert.Equal(QueueCategory.Unspecified, result.QueueCategory);
            Assert.Null(result.ProcessingStreamId);
            Assert.Null(result.InProgressAt);
            Assert.Equal(ReviewOrderStatus.Canceled, persisted.Status);
            Assert.Equal("ручная отмена", persisted.CancelReason);
            Assert.Equal(QueueCategory.Unspecified, persisted.QueueCategory);
            Assert.Null(persisted.ProcessingStreamId);
            Assert.Null(persisted.InProgressAt);
        }

        /// <summary>
        /// Проверяет, что для несуществующего заказа выбрасывается ошибка.
        /// </summary>
        [Fact]
        public async Task Cancel_Throws_WhenOrderDoesNotExist()
        {
            await using ApplicationTestHost app = await CreateAppAsync();

            await Assert.ThrowsAsync<ReviewOrderException>(() =>
                app.RunScopeAsync(services =>
                    services.GetRequiredService<ReviewOrderService>().Cancel(new CancelCommand
                    {
                        ReviewOrderId = long.MaxValue,
                        CancelReason = "поздняя отмена",
                    })));
        }
    }
}
