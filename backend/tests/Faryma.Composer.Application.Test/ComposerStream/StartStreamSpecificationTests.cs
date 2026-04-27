using Faryma.Composer.Application.Test.Infrastructure;

namespace Faryma.Composer.Application.Test.ComposerStream
{
    public sealed class StartStreamSpecificationTests(PostgreSqlFixture fixture) : ApplicationTestBase(fixture)
    {
        [Fact(Skip = "Use case requires rejecting stream start on a non-current date, but ComposerStreamService still has a TODO for this rule.")]
        public Task Start_ShouldRejectPlannedStream_WhenEventDateDoesNotMatchToday() => Task.CompletedTask;
    }
}
