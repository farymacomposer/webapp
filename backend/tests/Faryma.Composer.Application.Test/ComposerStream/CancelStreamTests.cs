using Faryma.Composer.Application.Features.ComposerStream;
using Faryma.Composer.Application.Test.Infrastructure;
using Faryma.Composer.Contracts.Infrastructure.Entities;
using Faryma.Composer.Contracts.Infrastructure.Enums;

namespace Faryma.Composer.Application.Test.ComposerStream
{
    public sealed class CancelStreamTests(PostgreSqlFixture fixture) : ApplicationTestBase(fixture)
    {
        /// <summary>
        /// Проверяет, что отмена переводит запланированный стрим в статус Canceled.
        /// </summary>
        [Fact]
        public async Task Cancel_TransitionsPlannedStreamToCanceled()
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("composer");
            ComposerStreamEntity stream = await app.Data.CreateStreamAsync(
                createdByUserId: user.Id,
                status: ComposerStreamStatus.Planned);

            ComposerStreamEntity result = await app.RunScopeAsync(services =>
                services.GetRequiredService<ComposerStreamService>().Cancel(stream.Id));
            ComposerStreamEntity persisted = await app.GetStreamAsync(stream.Id);

            Assert.Equal(ComposerStreamStatus.Canceled, result.Status);
            Assert.Equal(ComposerStreamStatus.Canceled, persisted.Status);
        }

        /// <summary>
        /// Проверяет, что повторная отмена уже отмененного стрима ничего не меняет.
        /// </summary>
        [Fact]
        public async Task Cancel_ReturnsCurrentStream_WhenAlreadyCanceled()
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("composer");
            ComposerStreamEntity stream = await app.Data.CreateStreamAsync(
                createdByUserId: user.Id,
                status: ComposerStreamStatus.Canceled);

            int beforeUpdates = app.QueueUpdateCount;
            ComposerStreamEntity result = await app.RunScopeAsync(services =>
                services.GetRequiredService<ComposerStreamService>().Cancel(stream.Id));
            ComposerStreamEntity persisted = await app.GetStreamAsync(stream.Id);

            Assert.Equal(stream.Id, result.Id);
            Assert.Equal(ComposerStreamStatus.Canceled, result.Status);
            Assert.Equal(ComposerStreamStatus.Canceled, persisted.Status);
            Assert.Equal(beforeUpdates, app.QueueUpdateCount);
        }

        /// <summary>
        /// Проверяет, что стрим нельзя отменить из недопустимого статуса.
        /// </summary>
        [Theory]
        [InlineData(ComposerStreamStatus.Live)]
        [InlineData(ComposerStreamStatus.Completed)]
        public async Task Cancel_Throws_WhenStreamHasInvalidStatus(ComposerStreamStatus status)
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("composer");
            ComposerStreamEntity stream = await app.Data.CreateStreamAsync(
                createdByUserId: user.Id,
                status: status,
                startedAt: status == ComposerStreamStatus.Live ? app.FixedNow : null,
                completedAt: status == ComposerStreamStatus.Completed ? app.FixedNow : null);

            await Assert.ThrowsAsync<ComposerStreamException>(() =>
                app.RunScopeAsync(services =>
                    services.GetRequiredService<ComposerStreamService>().Cancel(stream.Id)));
        }

        /// <summary>
        /// Проверяет, что стрим нельзя отменить при наличии активных созданных заказов.
        /// </summary>
        [Theory]
        [InlineData(ReviewOrderStatus.Preorder)]
        [InlineData(ReviewOrderStatus.Pending)]
        public async Task Cancel_Throws_WhenPlannedStreamHasActiveCreatedOrders(ReviewOrderStatus orderStatus)
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("composer");
            ComposerStreamEntity stream = await app.Data.CreateStreamAsync(
                createdByUserId: user.Id,
                status: ComposerStreamStatus.Planned);
            await app.Data.CreateReviewOrderAsync(
                createdByUserId: user.Id,
                creationStreamId: stream.Id,
                status: orderStatus);

            await Assert.ThrowsAsync<ComposerStreamException>(() =>
                app.RunScopeAsync(services =>
                    services.GetRequiredService<ComposerStreamService>().Cancel(stream.Id)));
        }

        /// <summary>
        /// Проверяет, что отмена несуществующего стрима завершается ошибкой.
        /// </summary>
        [Fact]
        public async Task Cancel_Throws_WhenStreamDoesNotExist()
        {
            await using ApplicationTestHost app = await CreateAppAsync();

            await Assert.ThrowsAsync<ComposerStreamException>(() =>
                app.RunScopeAsync(services =>
                    services.GetRequiredService<ComposerStreamService>().Cancel(long.MaxValue)));
        }
    }
}
