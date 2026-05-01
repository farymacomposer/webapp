namespace Faryma.Composer.Application.Test.Infrastructure
{
    [CollectionDefinition(nameof(CollectionFixture))]
    public sealed class CollectionFixture : ICollectionFixture<PostgreSqlFixture>;

    [Collection(nameof(CollectionFixture))]
    public abstract class TestBase(PostgreSqlFixture fixture)
    {
        protected Task<ApplicationTestHost> CreateAppAsync() => ApplicationTestHost.CreateAsync(fixture);
    }
}
