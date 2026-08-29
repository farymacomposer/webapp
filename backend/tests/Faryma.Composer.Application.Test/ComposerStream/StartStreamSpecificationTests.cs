using Faryma.Composer.Application.Test.Infrastructure;

namespace Faryma.Composer.Application.Test.ComposerStream
{
    public sealed class StartStreamSpecificationTests(PostgreSqlFixture fixture) : TestBase(fixture)
    {
        [Fact(Skip = "Сценарий требует отклонять старт стрима не за текущую дату, но в StartHandler для этого правила пока TODO.")]
        public Task Start_ShouldRejectPlannedStream_WhenEventDateDoesNotMatchToday() => Task.CompletedTask;
    }
}
