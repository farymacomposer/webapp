using Faryma.Composer.Application.Features.ComposerStream;
using Faryma.Composer.Application.Test.Infrastructure;
using Faryma.Composer.Contracts.Application.Features.ComposerStream.Commands;
using Faryma.Composer.Contracts.Infrastructure.Entities;
using Faryma.Composer.Contracts.Infrastructure.Enums;

namespace Faryma.Composer.Application.Test.ComposerStream
{
    public sealed class CreateStreamTests(PostgreSqlFixture fixture) : ApplicationTestBase(fixture)
    {
        [Fact]
        public async Task Create_CreatesPlannedStream()
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("composer");

            ComposerStreamEntity stream = await app.RunScopeAsync(services =>
                services.GetRequiredService<ComposerStreamService>().Create(new CreateCommand
                {
                    EventDate = app.Today.AddDays(5),
                    Type = ComposerStreamType.Donation,
                    CreatedByUserId = user.Id,
                }, CancellationToken.None));

            Assert.Equal(ComposerStreamStatus.Planned, stream.Status);
            Assert.Equal(ComposerStreamType.Donation, stream.Type);
            Assert.Null(stream.StartedAt);
            Assert.Null(stream.CompletedAt);
        }

        [Fact]
        public async Task Create_Throws_WhenStreamDateAlreadyExists()
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("composer");
            DateOnly eventDate = app.Today.AddDays(3);

            await app.Data.CreateStreamAsync(
                createdByUserId: user.Id,
                eventDate: eventDate,
                type: ComposerStreamType.Donation);

            await Assert.ThrowsAsync<ComposerStreamException>(() =>
                app.RunScopeAsync(services =>
                    services.GetRequiredService<ComposerStreamService>().Create(new CreateCommand
                    {
                        EventDate = eventDate,
                        Type = ComposerStreamType.Charity,
                        CreatedByUserId = user.Id,
                    }, CancellationToken.None)));
        }

        [Fact]
        public async Task Create_Throws_WhenUserDoesNotExist()
        {
            await using ApplicationTestHost app = await CreateAppAsync();

            await Assert.ThrowsAsync<ComposerStreamException>(() =>
                app.RunScopeAsync(services =>
                    services.GetRequiredService<ComposerStreamService>().Create(new CreateCommand
                    {
                        EventDate = app.Today.AddDays(1),
                        Type = ComposerStreamType.Donation,
                        CreatedByUserId = Guid.NewGuid(),
                    }, CancellationToken.None)));
        }
    }
}
