namespace Faryma.Composer.Api.Test.Infrastructure
{
    [TestClass]
    public static class PostgreSqlTestFixture
    {
        private static readonly SemaphoreSlim _gate = new(1, 1);
        private static PostgreSqlFixture? _fixture;

        public static async Task<PostgreSqlFixture> GetAsync()
        {
            if (_fixture is not null)
            {
                return _fixture;
            }

            await _gate.WaitAsync();
            try
            {
                if (_fixture is null)
                {
                    PostgreSqlFixture fixture = new();
                    await fixture.InitializeAsync();
                    _fixture = fixture;
                }
            }
            finally
            {
                _gate.Release();
            }

            return _fixture;
        }

        [AssemblyCleanup]
        public static async Task CleanupAsync()
        {
            if (_fixture is not null)
            {
                await _fixture.DisposeAsync();
                _fixture = null;
            }
        }
    }

    public abstract class DatabaseTestBase : TestBase
    {
        protected async Task<CustomWebApplicationFactory> CreateAppAsync()
        {
            PostgreSqlFixture fixture = await PostgreSqlTestFixture.GetAsync();

            return await CustomWebApplicationFactory.CreateAsync(fixture);
        }
    }

    public abstract class TestBase
    {
        public TestContext TestContext { get; set; } = null!;

        protected CustomWebApplicationFactory CreateApp() => CustomWebApplicationFactory.Create();
    }
}
