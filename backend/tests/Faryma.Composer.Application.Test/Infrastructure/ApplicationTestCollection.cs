namespace Faryma.Composer.Application.Test.Infrastructure
{
    [CollectionDefinition(nameof(ApplicationTestCollection))]
    public sealed class ApplicationTestCollection : ICollectionFixture<PostgreSqlFixture>;

    [Collection(nameof(ApplicationTestCollection))]
    public abstract class ApplicationTestBase(PostgreSqlFixture fixture)
    {
        protected Task<ApplicationTestHost> CreateAppAsync() => ApplicationTestHost.CreateAsync(fixture);
    }
}