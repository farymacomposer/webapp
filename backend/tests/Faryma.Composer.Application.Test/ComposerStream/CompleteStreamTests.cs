using Faryma.Composer.Application.Features.ComposerStream;
using Faryma.Composer.Application.Test.Infrastructure;
using Faryma.Composer.Contracts.Infrastructure.Entities;
using Faryma.Composer.Contracts.Infrastructure.Enums;

namespace Faryma.Composer.Application.Test.ComposerStream
{
    public sealed class CompleteStreamTests(PostgreSqlFixture fixture) : ApplicationTestBase(fixture)
    {
        /// <summary>
        /// Проверяет, что завершение переводит активный стрим в статус Completed.
        /// </summary>
        [Fact]
        public async Task Complete_TransitionsLiveStreamToCompleted()
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("composer");
            ComposerStreamEntity stream = await app.Data.CreateStreamAsync(
                createdByUserId: user.Id,
                status: ComposerStreamStatus.Live,
                startedAt: app.FixedNow);

            ComposerStreamEntity result = await app.RunScopeAsync(services =>
                services.GetRequiredService<ComposerStreamService>().Complete(stream.Id));

            ComposerStreamEntity persisted = await app.GetStreamAsync(stream.Id);
            Assert.Equal(ComposerStreamStatus.Completed, result.Status);
            Assert.Equal(ComposerStreamStatus.Completed, persisted.Status);
            Assert.Equal(app.FixedNow, persisted.CompletedAt);
        }

        /// <summary>
        /// Проверяет, что повторное завершение уже завершенного стрима ничего не меняет.
        /// </summary>
        [Fact]
        public async Task Complete_ReturnsCurrentStream_WhenAlreadyCompleted()
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("composer");
            DateTime originalCompletedAt = app.FixedNow.AddMinutes(-5);
            ComposerStreamEntity stream = await app.Data.CreateStreamAsync(
                createdByUserId: user.Id,
                status: ComposerStreamStatus.Completed,
                completedAt: originalCompletedAt);

            int beforeUpdates = app.QueueUpdateCount;
            ComposerStreamEntity result = await app.RunScopeAsync(services =>
                services.GetRequiredService<ComposerStreamService>().Complete(stream.Id));
            ComposerStreamEntity persisted = await app.GetStreamAsync(stream.Id);

            Assert.Equal(stream.Id, result.Id);
            Assert.Equal(originalCompletedAt, result.CompletedAt);
            Assert.Equal(originalCompletedAt, persisted.CompletedAt);
            Assert.Equal(ComposerStreamStatus.Completed, persisted.Status);
            Assert.Equal(beforeUpdates, app.QueueUpdateCount);
        }

        /// <summary>
        /// Проверяет, что стрим нельзя завершить при наличии заказа в работе.
        /// </summary>
        [Fact]
        public async Task Complete_Throws_WhenOrderIsInProgress()
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("composer");
            ComposerStreamEntity stream = await app.Data.CreateStreamAsync(
                createdByUserId: user.Id,
                status: ComposerStreamStatus.Live,
                startedAt: app.FixedNow);
            await app.Data.CreateReviewOrderAsync(
                createdByUserId: user.Id,
                creationStreamId: stream.Id,
                processingStreamId: stream.Id,
                status: ReviewOrderStatus.InProgress,
                inProgressAt: app.FixedNow);

            await Assert.ThrowsAsync<ComposerStreamException>(() =>
                app.RunScopeAsync(services =>
                    services.GetRequiredService<ComposerStreamService>().Complete(stream.Id)));
        }

        /// <summary>
        /// Проверяет, что завершение блокируется, если любой заказ остается в работе даже на другом стриме.
        /// </summary>
        [Fact]
        public async Task Complete_Throws_WhenAnyOrderIsInProgress_EvenOnAnotherStream()
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("composer");
            ComposerStreamEntity liveStream = await app.Data.CreateStreamAsync(
                createdByUserId: user.Id,
                eventDate: app.Today,
                status: ComposerStreamStatus.Live,
                startedAt: app.FixedNow);
            ComposerStreamEntity otherStream = await app.Data.CreateStreamAsync(
                createdByUserId: user.Id,
                eventDate: app.Today.AddDays(1),
                status: ComposerStreamStatus.Completed,
                completedAt: app.FixedNow);

            await app.Data.CreateReviewOrderAsync(
                createdByUserId: user.Id,
                creationStreamId: liveStream.Id,
                processingStreamId: otherStream.Id,
                status: ReviewOrderStatus.InProgress,
                inProgressAt: app.FixedNow);

            await Assert.ThrowsAsync<ComposerStreamException>(() =>
                app.RunScopeAsync(services =>
                    services.GetRequiredService<ComposerStreamService>().Complete(liveStream.Id)));
        }

        /// <summary>
        /// Проверяет, что завершение недоступно для стрима в недопустимом статусе.
        /// </summary>
        [Theory]
        [InlineData(ComposerStreamStatus.Planned)]
        [InlineData(ComposerStreamStatus.Canceled)]
        public async Task Complete_Throws_WhenStreamHasInvalidStatus(ComposerStreamStatus status)
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("composer");
            ComposerStreamEntity stream = await app.Data.CreateStreamAsync(
                createdByUserId: user.Id,
                status: status);

            await Assert.ThrowsAsync<ComposerStreamException>(() =>
                app.RunScopeAsync(services =>
                    services.GetRequiredService<ComposerStreamService>().Complete(stream.Id)));
        }

        /// <summary>
        /// Проверяет, что завершение несуществующего стрима завершается ошибкой.
        /// </summary>
        [Fact]
        public async Task Complete_Throws_WhenStreamDoesNotExist()
        {
            await using ApplicationTestHost app = await CreateAppAsync();

            await Assert.ThrowsAsync<ComposerStreamException>(() =>
                app.RunScopeAsync(services =>
                    services.GetRequiredService<ComposerStreamService>().Complete(long.MaxValue)));
        }
    }
}
