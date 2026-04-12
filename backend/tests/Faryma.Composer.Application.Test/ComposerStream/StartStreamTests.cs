using Faryma.Composer.Application.Features.ComposerStream;
using Faryma.Composer.Application.Test.Infrastructure;
using Faryma.Composer.Contracts.Application.Features.ComposerStream;
using Faryma.Composer.Contracts.Infrastructure.Entities;
using Faryma.Composer.Contracts.Infrastructure.Enums;

namespace Faryma.Composer.Application.Test.ComposerStream
{
    public sealed class StartStreamTests(PostgreSqlFixture fixture) : ApplicationTestBase(fixture)
    {
        [Fact]
        public async Task Start_TransitionsPlannedStreamToLive()
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("composer");
            ComposerStreamEntity stream = await app.Data.CreateStreamAsync(
                createdByUserId: user.Id,
                eventDate: app.Today,
                type: ComposerStreamType.Donation,
                status: ComposerStreamStatus.Planned);

            int expectedUpdates = app.QueueUpdateCount + 1;
            ComposerStreamEntity result = await app.RunScopeAsync(services =>
                services.GetRequiredService<ComposerStreamService>().Start(stream.Id, CancellationToken.None));

            await app.WaitForQueueUpdateCountAsync(expectedUpdates);

            ComposerStreamEntity persisted = await app.GetStreamAsync(stream.Id);
            Assert.Equal(ComposerStreamStatus.Live, result.Status);
            Assert.Equal(ComposerStreamStatus.Live, persisted.Status);
            Assert.Equal(app.FixedNow, persisted.StartedAt);
        }

        [Fact]
        public async Task Start_ReturnsCurrentStream_WhenAlreadyLive()
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("composer");
            ComposerStreamEntity stream = await app.Data.CreateStreamAsync(
                createdByUserId: user.Id,
                status: ComposerStreamStatus.Live,
                startedAt: app.FixedNow);

            int beforeUpdates = app.QueueUpdateCount;
            ComposerStreamEntity result = await app.RunScopeAsync(services =>
                services.GetRequiredService<ComposerStreamService>().Start(stream.Id, CancellationToken.None));

            Assert.Equal(stream.Id, result.Id);
            Assert.Equal(beforeUpdates, app.QueueUpdateCount);
        }

        [Fact]
        public async Task Start_Throws_WhenAnotherLiveStreamExists()
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("composer");
            await app.Data.CreateStreamAsync(
                createdByUserId: user.Id,
                status: ComposerStreamStatus.Live,
                startedAt: app.FixedNow);
            ComposerStreamEntity stream = await app.Data.CreateStreamAsync(
                createdByUserId: user.Id,
                status: ComposerStreamStatus.Planned);

            await Assert.ThrowsAsync<ComposerStreamException>(() =>
                app.RunScopeAsync(services =>
                    services.GetRequiredService<ComposerStreamService>().Start(stream.Id, CancellationToken.None)));
        }

        [Theory]
        [InlineData(ComposerStreamStatus.Completed)]
        [InlineData(ComposerStreamStatus.Canceled)]
        public async Task Start_Throws_WhenStreamHasInvalidStatus(ComposerStreamStatus status)
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("composer");
            ComposerStreamEntity stream = await app.Data.CreateStreamAsync(
                createdByUserId: user.Id,
                status: status,
                completedAt: status == ComposerStreamStatus.Completed ? app.FixedNow : null);

            await Assert.ThrowsAsync<ComposerStreamException>(() =>
                app.RunScopeAsync(services =>
                    services.GetRequiredService<ComposerStreamService>().Start(stream.Id, CancellationToken.None)));
        }
    }
}
