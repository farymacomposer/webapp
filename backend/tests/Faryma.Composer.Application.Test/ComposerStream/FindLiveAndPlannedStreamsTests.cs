using Faryma.Composer.Application.Features.ComposerStream;
using Faryma.Composer.Application.Test.Infrastructure;
using Faryma.Composer.Contracts.Infrastructure.Entities;
using Faryma.Composer.Contracts.Infrastructure.Enums;

namespace Faryma.Composer.Application.Test.ComposerStream
{
    public sealed class FindLiveAndPlannedStreamsTests(PostgreSqlFixture fixture) : ApplicationTestBase(fixture)
    {
        [Fact]
        public async Task FindLiveAndPlanned_ReturnsOnlyLiveAndFuturePlannedStreams()
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("composer");

            ComposerStreamEntity live = await app.Data.CreateStreamAsync(
                createdByUserId: user.Id,
                eventDate: app.Today.AddDays(-2),
                type: ComposerStreamType.Donation,
                status: ComposerStreamStatus.Live,
                startedAt: app.FixedNow);

            ComposerStreamEntity plannedFuture = await app.Data.CreateStreamAsync(
                createdByUserId: user.Id,
                eventDate: app.Today.AddDays(2),
                type: ComposerStreamType.Charity,
                status: ComposerStreamStatus.Planned);

            ComposerStreamEntity plannedToday = await app.Data.CreateStreamAsync(
                createdByUserId: user.Id,
                eventDate: app.Today,
                type: ComposerStreamType.Donation,
                status: ComposerStreamStatus.Planned);

            ComposerStreamEntity plannedPast = await app.Data.CreateStreamAsync(
                createdByUserId: user.Id,
                eventDate: app.Today.AddDays(-1),
                type: ComposerStreamType.Debt,
                status: ComposerStreamStatus.Planned);

            ComposerStreamEntity canceled = await app.Data.CreateStreamAsync(
                createdByUserId: user.Id,
                eventDate: app.Today.AddDays(1),
                type: ComposerStreamType.Charity,
                status: ComposerStreamStatus.Canceled);

            ComposerStreamEntity completed = await app.Data.CreateStreamAsync(
                createdByUserId: user.Id,
                eventDate: app.Today.AddDays(-3),
                type: ComposerStreamType.Donation,
                status: ComposerStreamStatus.Completed,
                completedAt: app.FixedNow);

            List<ComposerStreamEntity> result = await app.RunScopeAsync(services =>
                services.GetRequiredService<ComposerStreamService>().FindLiveAndPlanned(CancellationToken.None));

            long[] ids = result.Select(x => x.Id).ToArray();
            Assert.Contains(live.Id, ids);
            Assert.Contains(plannedFuture.Id, ids);
            Assert.Contains(plannedToday.Id, ids);
            Assert.DoesNotContain(plannedPast.Id, ids);
            Assert.DoesNotContain(canceled.Id, ids);
            Assert.DoesNotContain(completed.Id, ids);
        }
    }
}