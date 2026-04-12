using Faryma.Composer.Application.Features.ComposerStream;
using Faryma.Composer.Application.Test.Infrastructure;
using Faryma.Composer.Contracts.Infrastructure.Entities;
using Faryma.Composer.Contracts.Infrastructure.Enums;

namespace Faryma.Composer.Application.Test.ComposerStream
{
    public sealed class FindStreamsTests(PostgreSqlFixture fixture) : ApplicationTestBase(fixture)
    {
        [Fact]
        public async Task Find_ReturnsStreamsInsideRequestedPeriod()
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("composer");
            ComposerStreamEntity before = await app.Data.CreateStreamAsync(
                createdByUserId: user.Id,
                eventDate: app.Today.AddDays(-1),
                type: ComposerStreamType.Donation);
            ComposerStreamEntity fromPeriod = await app.Data.CreateStreamAsync(
                createdByUserId: user.Id,
                eventDate: app.Today,
                type: ComposerStreamType.Charity);
            ComposerStreamEntity toPeriod = await app.Data.CreateStreamAsync(
                createdByUserId: user.Id,
                eventDate: app.Today.AddDays(2),
                type: ComposerStreamType.Debt);

            List<ComposerStreamEntity> result = await app.RunScopeAsync(services =>
                services.GetRequiredService<ComposerStreamService>().Find(app.Today, app.Today.AddDays(2), CancellationToken.None));

            long[] ids = result.Select(x => x.Id).ToArray();
            Assert.DoesNotContain(before.Id, ids);
            Assert.Contains(fromPeriod.Id, ids);
            Assert.Contains(toPeriod.Id, ids);
        }
    }
}
