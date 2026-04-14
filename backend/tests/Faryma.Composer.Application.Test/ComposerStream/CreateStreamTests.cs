using System.Diagnostics;
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
            DateOnly eventDate = app.Today.AddDays(5);

            ComposerStreamEntity stream = await app.RunScopeAsync(services =>
                services.GetRequiredService<ComposerStreamService>().Create(new CreateCommand
                {
                    EventDate = eventDate,
                    Type = ComposerStreamType.Donation,
                    CreatedByUserId = user.Id,
                }));
            ComposerStreamEntity persisted = await app.GetStreamAsync(stream.Id);

            Assert.Equal(ComposerStreamStatus.Planned, stream.Status);
            Assert.Equal(ComposerStreamType.Donation, stream.Type);
            Assert.Null(stream.StartedAt);
            Assert.Null(stream.CompletedAt);
            Assert.Equal(eventDate, persisted.EventDate);
            Assert.Equal(ComposerStreamStatus.Planned, persisted.Status);
            Assert.Equal(ComposerStreamType.Donation, persisted.Type);
            Assert.Null(persisted.StartedAt);
            Assert.Null(persisted.CompletedAt);
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
                    })));
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
                    })));
        }

        [Fact]
        public async Task Create_Throws_WhenTypeIsUnspecified()
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("composer");

            await Assert.ThrowsAsync<UnreachableException>(() =>
                app.RunScopeAsync(services =>
                    services.GetRequiredService<ComposerStreamService>().Create(new CreateCommand
                    {
                        EventDate = app.Today.AddDays(1),
                        Type = ComposerStreamType.Unspecified,
                        CreatedByUserId = user.Id,
                    })));
        }
    }
}