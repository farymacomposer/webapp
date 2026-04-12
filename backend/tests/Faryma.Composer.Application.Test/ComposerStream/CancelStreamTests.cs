using Faryma.Composer.Application.Features.ComposerStream;
using Faryma.Composer.Application.Test.Infrastructure;
using Faryma.Composer.Contracts.Infrastructure.Entities;
using Faryma.Composer.Contracts.Infrastructure.Enums;

namespace Faryma.Composer.Application.Test.ComposerStream
{
    public sealed class CancelStreamTests(PostgreSqlFixture fixture) : ApplicationTestBase(fixture)
    {
        [Fact]
        public async Task Cancel_TransitionsPlannedStreamToCanceled()
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("composer");
            ComposerStreamEntity stream = await app.Data.CreateStreamAsync(
                createdByUserId: user.Id,
                status: ComposerStreamStatus.Planned);

            ComposerStreamEntity result = await app.RunScopeAsync(services =>
                services.GetRequiredService<ComposerStreamService>().Cancel(stream.Id, CancellationToken.None));
            ComposerStreamEntity persisted = await app.GetStreamAsync(stream.Id);

            Assert.Equal(ComposerStreamStatus.Canceled, result.Status);
            Assert.Equal(ComposerStreamStatus.Canceled, persisted.Status);
        }

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
                services.GetRequiredService<ComposerStreamService>().Cancel(stream.Id, CancellationToken.None));
            ComposerStreamEntity persisted = await app.GetStreamAsync(stream.Id);

            Assert.Equal(stream.Id, result.Id);
            Assert.Equal(ComposerStreamStatus.Canceled, result.Status);
            Assert.Equal(ComposerStreamStatus.Canceled, persisted.Status);
            Assert.Equal(beforeUpdates, app.QueueUpdateCount);
        }

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
                    services.GetRequiredService<ComposerStreamService>().Cancel(stream.Id, CancellationToken.None)));
        }

        [Fact]
        public async Task Cancel_Throws_WhenStreamDoesNotExist()
        {
            await using ApplicationTestHost app = await CreateAppAsync();

            await Assert.ThrowsAsync<ComposerStreamException>(() =>
                app.RunScopeAsync(services =>
                    services.GetRequiredService<ComposerStreamService>().Cancel(long.MaxValue, CancellationToken.None)));
        }
    }
}