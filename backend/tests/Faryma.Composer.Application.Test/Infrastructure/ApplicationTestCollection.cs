namespace Faryma.Composer.Application.Test.Infrastructure
{
    /// <summary>
    /// Объединяет application-тесты в общую коллекцию с PostgreSQL fixture.
    /// </summary>
    [CollectionDefinition(nameof(ApplicationTestCollection))]
    public sealed class ApplicationTestCollection : ICollectionFixture<PostgreSqlFixture>;

    /// <summary>
    /// Дает базовым тестам доступ к созданию изолированного приложения.
    /// </summary>
    [Collection(nameof(ApplicationTestCollection))]
    public abstract class ApplicationTestBase(PostgreSqlFixture fixture)
    {
        protected Task<ApplicationTestHost> CreateAppAsync() => ApplicationTestHost.CreateAsync(fixture);
    }
}
