using Faryma.Composer.Application.Test.Infrastructure;

namespace Faryma.Composer.Application.Test.Specification
{
    public sealed class CancelStreamSpecificationTests(PostgreSqlFixture fixture) : ApplicationTestBase(fixture)
    {
        [Fact(Skip = "Use case requires rejecting stream cancel when orders already exist for the stream, but ComposerStreamService still has a TODO for this rule.")]
        public Task Cancel_ShouldRejectPlannedStream_WhenOrdersAlreadyExist() => Task.CompletedTask;
    }
}
