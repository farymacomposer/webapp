namespace Faryma.Composer.Api.Test.Infrastructure
{
    [CollectionDefinition(nameof(CollectionFixture))]
    public sealed class CollectionFixture : ICollectionFixture<PostgreSqlFixture>;

    [Collection(nameof(CollectionFixture))]
    public abstract class TestBase(PostgreSqlFixture fixture)
    {
        protected Task<CustomWebApplicationFactory> CreateAppAsync() => CustomWebApplicationFactory.CreateAsync(fixture);
    }
}
