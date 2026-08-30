namespace Faryma.Composer.Testing.Infrastructure
{
    [CollectionDefinition(nameof(ApplicationCollectionFixture))]
    public sealed class ApplicationCollectionFixture : ICollectionFixture<PostgreSqlFixture>;

    [Collection(nameof(ApplicationCollectionFixture))]
    public abstract class ApplicationTestBase(PostgreSqlFixture fixture)
    {
        protected Task<ApplicationTestHost> CreateAppAsync() => ApplicationTestHost.CreateAsync(fixture);
    }
}
