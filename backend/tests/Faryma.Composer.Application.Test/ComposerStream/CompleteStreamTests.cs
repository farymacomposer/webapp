using Faryma.Composer.Application.Features.ComposerStream;
using Faryma.Composer.Application.Test.Infrastructure;
using Faryma.Composer.Contracts.Infrastructure.Entities;
using Faryma.Composer.Contracts.Infrastructure.Enums;

namespace Faryma.Composer.Application.Test.ComposerStream
{
    public sealed class CompleteStreamTests(PostgreSqlFixture fixture) : ApplicationTestBase(fixture)
    {
        [Fact]
        public async Task Complete_TransitionsLiveStreamToCompleted()
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("composer");
            ComposerStreamEntity stream = await app.Data.CreateStreamAsync(
                createdByUserId: user.Id,
                status: ComposerStreamStatus.Live,
                startedAt: app.FixedNow);

            int expectedUpdates = app.QueueUpdateCount + 1;
            ComposerStreamEntity result = await app.RunScopeAsync(services =>
                services.GetRequiredService<ComposerStreamService>().Complete(stream.Id, CancellationToken.None));

            await app.WaitForQueueUpdateCountAsync(expectedUpdates);

            ComposerStreamEntity persisted = await app.GetStreamAsync(stream.Id);
            Assert.Equal(ComposerStreamStatus.Completed, result.Status);
            Assert.Equal(ComposerStreamStatus.Completed, persisted.Status);
            Assert.Equal(app.FixedNow, persisted.CompletedAt);
        }

        [Fact]
        public async Task Complete_ReturnsCurrentStream_WhenAlreadyCompleted()
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("composer");
            ComposerStreamEntity stream = await app.Data.CreateStreamAsync(
                createdByUserId: user.Id,
                status: ComposerStreamStatus.Completed,
                completedAt: app.FixedNow);

            int beforeUpdates = app.QueueUpdateCount;
            ComposerStreamEntity result = await app.RunScopeAsync(services =>
                services.GetRequiredService<ComposerStreamService>().Complete(stream.Id, CancellationToken.None));

            Assert.Equal(stream.Id, result.Id);
            Assert.Equal(beforeUpdates, app.QueueUpdateCount);
        }

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
                    services.GetRequiredService<ComposerStreamService>().Complete(stream.Id, CancellationToken.None)));
        }

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
                    services.GetRequiredService<ComposerStreamService>().Complete(stream.Id, CancellationToken.None)));
        }
    }
}