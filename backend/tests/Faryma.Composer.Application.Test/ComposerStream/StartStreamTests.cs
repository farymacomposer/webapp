using Faryma.Composer.Application.Features.ComposerStream.Start;
using Faryma.Composer.Application.Test.Infrastructure;
using Faryma.Composer.Domain.Entities;
using Faryma.Composer.Domain.Enums;
using Faryma.Composer.Domain.Exceptions;

namespace Faryma.Composer.Application.Test.ComposerStream
{
    public sealed class StartStreamTests(PostgreSqlFixture fixture) : TestBase(fixture)
    {
        /// <summary>
        /// Проверяет, что запуск переводит запланированный стрим в статус Live.
        /// </summary>
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

            ComposerStreamEntity result = await app.SendAsync(new StartCommand
            {
                ComposerStreamId = stream.Id,
            });

            ComposerStreamEntity persisted = await app.GetStreamAsync(stream.Id);
            Assert.Equal(ComposerStreamStatus.Live, result.Status);
            Assert.Equal(ComposerStreamStatus.Live, persisted.Status);
            Assert.Equal(app.FixedNow, persisted.StartedAt);
        }

        /// <summary>
        /// Проверяет, что повторный запуск уже активного стрима ничего не меняет.
        /// </summary>
        [Fact]
        public async Task Start_ReturnsCurrentStream_WhenAlreadyLive()
        {
            await using ApplicationTestHost app = await CreateAppAsync();
            UserEntity user = await app.Data.CreateUserAsync("composer");
            DateTime originalStartedAt = app.FixedNow.AddMinutes(-5);
            ComposerStreamEntity stream = await app.Data.CreateStreamAsync(
                createdByUserId: user.Id,
                status: ComposerStreamStatus.Live,
                startedAt: originalStartedAt);

            int beforeUpdates = app.QueueUpdateCount;
            ComposerStreamEntity result = await app.SendAsync(new StartCommand
            {
                ComposerStreamId = stream.Id,
            });
            ComposerStreamEntity persisted = await app.GetStreamAsync(stream.Id);

            Assert.Equal(stream.Id, result.Id);
            Assert.Equal(originalStartedAt, result.StartedAt);
            Assert.Equal(originalStartedAt, persisted.StartedAt);
            Assert.Equal(ComposerStreamStatus.Live, persisted.Status);
            Assert.Equal(beforeUpdates, app.QueueUpdateCount);
        }

        /// <summary>
        /// Проверяет, что нельзя запустить стрим, пока уже существует другой активный стрим.
        /// </summary>
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
                app.SendAsync(new StartCommand
                {
                    ComposerStreamId = stream.Id,
                }));
        }

        /// <summary>
        /// Проверяет, что запуск недоступен для стрима в недопустимом статусе.
        /// </summary>
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
                app.SendAsync(new StartCommand
                {
                    ComposerStreamId = stream.Id,
                }));
        }

        /// <summary>
        /// Проверяет, что запуск несуществующего стрима завершается ошибкой.
        /// </summary>
        [Fact]
        public async Task Start_Throws_WhenStreamDoesNotExist()
        {
            await using ApplicationTestHost app = await CreateAppAsync();

            await Assert.ThrowsAsync<NotFoundException>(() =>
                app.SendAsync(new StartCommand
                {
                    ComposerStreamId = long.MaxValue,
                }));
        }
    }
}
