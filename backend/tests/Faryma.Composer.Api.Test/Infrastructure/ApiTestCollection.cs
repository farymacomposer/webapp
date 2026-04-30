namespace Faryma.Composer.Api.Test.Infrastructure
{
    /// <summary>
    /// Объединяет API integration tests в общую коллекцию с PostgreSQL fixture.
    /// </summary>
    [CollectionDefinition(nameof(ApiTestCollection))]
    public sealed class ApiTestCollection : ICollectionFixture<PostgreSqlFixture>;

    /// <summary>
    /// Дает базовым тестам доступ к созданию изолированного test host.
    /// </summary>
    [Collection(nameof(ApiTestCollection))]
    public abstract class ApiTestBase(PostgreSqlFixture fixture)
    {
        protected Task<CustomWebApplicationFactory> CreateAppAsync() => CustomWebApplicationFactory.CreateAsync(fixture);
    }
}
