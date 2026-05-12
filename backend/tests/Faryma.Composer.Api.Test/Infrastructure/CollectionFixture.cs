namespace Faryma.Composer.Api.Test.Infrastructure
{
    [CollectionDefinition(nameof(CollectionFixture))]
    public sealed class CollectionFixture : ICollectionFixture<PostgreSqlFixture>;

    [Collection(nameof(CollectionFixture))]
    public abstract class DatabaseTestBase(PostgreSqlFixture fixture)
    {
        protected Task<CustomWebApplicationFactory> CreateAppAsync() => CustomWebApplicationFactory.CreateAsync(fixture);
    }

    public abstract class TestBase
    {
        protected CustomWebApplicationFactory CreateApp() => CustomWebApplicationFactory.Create();
    }
}
