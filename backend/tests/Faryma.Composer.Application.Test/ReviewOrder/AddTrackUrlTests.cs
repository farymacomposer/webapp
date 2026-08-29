using Faryma.Composer.Application.Features.AppSettings;
using Faryma.Composer.Application.Features.ReviewOrder.AddTrackUrl;
using Faryma.Composer.Infrastructure.Features.ReviewOrder;
using Faryma.Composer.Application.Test.Infrastructure;
using Faryma.Composer.Domain.Entities;
using Faryma.Composer.Domain.Entities.TransactionSources;
using Faryma.Composer.Domain.Enums;
using Faryma.Composer.Domain.Exceptions;

namespace Faryma.Composer.Application.Test.ReviewOrder
{
    public sealed class AddTrackUrlTests(PostgreSqlFixture fixture) : TestBase(fixture)
    {
        /// <summary>
        /// Проверяет, что добавление ссылки и длительности переводит покрытый предзаказ в статус Pending.
        /// </summary>
        [Fact]
        public async Task AddTrackUrl_MovesCoveredPreorderToPending()
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("admin");
            ReviewOrderEntity order = await app.Data.CreateReviewOrderAsync(
                createdByUserId: user.Id,
                status: ReviewOrderStatus.Preorder,
                trackUrl: null,
                totalPaymentAmount: 750);

            ReviewOrderEntity result = await app.RunScopeAsync(services =>
                services.Send(new AddTrackUrlCommand
                {
                    ReviewOrderId = order.Id,
                    TrackUrl = "https://example.com/new-track",
                    TrackDurationSeconds = 60,
                }));
            ReviewOrderEntity persisted = await app.GetOrderAsync(order.Id);

            Assert.Equal(ReviewOrderStatus.AwaitingPayment, result.Status);
            Assert.Equal("https://example.com/new-track", result.TrackUrl);
            Assert.Equal(60, result.TrackDurationSeconds);
            Assert.Equal(ReviewOrderStatus.AwaitingPayment, persisted.Status);
            Assert.Equal("https://example.com/new-track", persisted.TrackUrl);
            Assert.Equal(60, persisted.TrackDurationSeconds);
        }

        /// <summary>
        /// Проверяет, что добавление длинного трека переводит частично покрытый предзаказ в ожидание оплаты.
        /// </summary>
        [Fact]
        public async Task AddTrackUrl_MovesPartiallyCoveredPreorderToAwaitingPayment()
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("admin");
            ReviewOrderEntity order = await app.Data.CreateReviewOrderAsync(
                createdByUserId: user.Id,
                status: ReviewOrderStatus.Preorder,
                trackUrl: null,
                totalPaymentAmount: 750);

            ReviewOrderEntity result = await app.RunScopeAsync(services =>
                services.Send(new AddTrackUrlCommand
                {
                    ReviewOrderId = order.Id,
                    TrackUrl = "https://example.com/long-track",
                    TrackDurationSeconds = 420,
                }));
            ReviewOrderEntity persisted = await app.GetOrderAsync(order.Id);

            Assert.Equal(ReviewOrderStatus.AwaitingPayment, result.Status);
            Assert.Equal("https://example.com/long-track", result.TrackUrl);
            Assert.Equal(420, result.TrackDurationSeconds);
            Assert.Equal(ReviewOrderStatus.AwaitingPayment, persisted.Status);
            Assert.Equal("https://example.com/long-track", persisted.TrackUrl);
            Assert.Equal(420, persisted.TrackDurationSeconds);
        }

        /// <summary>
        /// Проверяет, что snapshot обязательной стоимости обновляется при изменении длительности трека.
        /// </summary>
        [Fact]
        public async Task AddTrackUrl_UpdatesPayableAmountSnapshot_WhenDurationChanges()
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("admin");
            ReviewOrderEntity order = await app.Data.CreateReviewOrderAsync(
                createdByUserId: user.Id,
                status: ReviewOrderStatus.Pending,
                trackDurationSeconds: 60,
                payableAmount: 750,
                totalPaymentAmount: 1_200);

            ReviewOrderEntity result = await app.RunScopeAsync(services =>
                services.Send(new AddTrackUrlCommand
                {
                    ReviewOrderId = order.Id,
                    TrackUrl = "https://example.com/longer-track",
                    TrackDurationSeconds = 420,
                }));
            ReviewOrderEntity persisted = await app.GetOrderAsync(order.Id);

            Assert.Equal(160, result.PayableAmount);
            Assert.Equal(160, persisted.PayableAmount);
        }

        /// <summary>
        /// Проверяет, что AddTrackUrl фиксирует новый snapshot стоимости по настройкам на момент добавления трека.
        /// </summary>
        [Fact]
        public async Task AddTrackUrl_SnapshotsPayableAmountUsingCurrentSettings()
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("admin");
            ReviewOrderEntity order = await app.Data.CreateReviewOrderAsync(
                createdByUserId: user.Id,
                status: ReviewOrderStatus.Preorder,
                trackUrl: null,
                nominalPrice: 750,
                payableAmount: 750,
                totalPaymentAmount: 1_350);

            ReviewOrderEntity result = await app.RunScopeAsync(async services =>
            {
                await ConfigurePricing(services, extraTimeAmountPerSecond: 5, detailedReviewAmount: 1_000);

                return await services.Send(new AddTrackUrlCommand
                {
                    ReviewOrderId = order.Id,
                    TrackUrl = "https://example.com/snapshot-track",
                    TrackDurationSeconds = 420,
                });
            });

            ReviewOrderEntity persistedAfterSettingsChange = await app.RunScopeAsync(async services =>
            {
                await ConfigurePricing(services, extraTimeAmountPerSecond: 1, detailedReviewAmount: 1_000);

                return await services.GetRequiredService<ReviewOrderStore>().FindOrderById(order.Id, CancellationToken.None)
                    ?? throw new InvalidOperationException("Заказ не найден");
            });

            Assert.Equal(ReviewOrderStatus.AwaitingPayment, result.Status);
            Assert.Equal(250, result.PayableAmount);
            Assert.Equal(250, persistedAfterSettingsChange.PayableAmount);
        }

        /// <summary>
        /// Проверяет, что ссылка обновляется без смены допустимого статуса заказа.
        /// </summary>
        [Theory]
        [InlineData(ReviewOrderStatus.Pending)]
        [InlineData(ReviewOrderStatus.AwaitingPayment)]
        public async Task AddTrackUrl_UpdatesUrlWithoutChangingStatus(ReviewOrderStatus status)
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("admin");
            ReviewOrderEntity order = await app.Data.CreateReviewOrderAsync(
                createdByUserId: user.Id,
                status: status,
                totalPaymentAmount: status == ReviewOrderStatus.Pending ? 750 : 0);

            ReviewOrderEntity result = await app.RunScopeAsync(services =>
                services.Send(new AddTrackUrlCommand
                {
                    ReviewOrderId = order.Id,
                    TrackUrl = "https://example.com/updated-track",
                    TrackDurationSeconds = 60,
                }));
            ReviewOrderEntity persisted = await app.GetOrderAsync(order.Id);

            ReviewOrderStatus expectedStatus = status == ReviewOrderStatus.Pending
                ? ReviewOrderStatus.AwaitingPayment
                : status;

            Assert.Equal(expectedStatus, result.Status);
            Assert.Equal("https://example.com/updated-track", result.TrackUrl);
            Assert.Equal(60, result.TrackDurationSeconds);
            Assert.Equal(expectedStatus, persisted.Status);
            Assert.Equal("https://example.com/updated-track", persisted.TrackUrl);
            Assert.Equal(60, persisted.TrackDurationSeconds);
        }

        /// <summary>
        /// Проверяет, что у заказа в работе, завершенного или отмененного заказа ссылку менять нельзя.
        /// </summary>
        [Theory]
        [InlineData(ReviewOrderStatus.InProgress)]
        [InlineData(ReviewOrderStatus.Completed)]
        [InlineData(ReviewOrderStatus.Canceled)]
        public async Task AddTrackUrl_Throws_WhenOrderHasInvalidStatus(ReviewOrderStatus status)
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
                reviewRating: status == ReviewOrderStatus.Completed ? 9 : null);

            await Assert.ThrowsAsync<ReviewOrderException>(() =>
                app.RunScopeAsync(services =>
                    services.Send(new AddTrackUrlCommand
                    {
                        ReviewOrderId = order.Id,
                        TrackUrl = "https://example.com/fail-track",
                        TrackDurationSeconds = 60,
                    })));
        }

        /// <summary>
        /// Проверяет, что для несуществующего заказа выбрасывается ошибка.
        /// </summary>
        [Fact]
        public async Task AddTrackUrl_Throws_WhenOrderDoesNotExist()
        {
            await using ApplicationTestHost app = await CreateAppAsync();

            await Assert.ThrowsAsync<NotFoundException>(() =>
                app.RunScopeAsync(services =>
                    services.Send(new AddTrackUrlCommand
                    {
                        ReviewOrderId = long.MaxValue,
                        TrackUrl = "https://example.com/missing-track",
                        TrackDurationSeconds = 60,
                    })));
        }

        private static async Task ConfigurePricing(
            IServiceProvider services,
            long extraTimeAmountPerSecond,
            long detailedReviewAmount)
        {
            AppSettingsService appSettingsService = services.GetRequiredService<AppSettingsService>();
            await appSettingsService.Update(new AppSettingsEntity
            {
                ReviewOrderNominalPrice = appSettingsService.Settings.ReviewOrderNominalPrice,
                IncludedTrackDurationSeconds = appSettingsService.Settings.IncludedTrackDurationSeconds,
                ReviewOrderExtraTrackSecondPrice = extraTimeAmountPerSecond,
                ReviewOrderDetailedPrice = detailedReviewAmount,
            }, CancellationToken.None);
        }
    }
}
