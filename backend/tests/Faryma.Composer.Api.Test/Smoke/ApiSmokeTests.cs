using Faryma.Composer.Api.Test.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Faryma.Composer.Api.Test.Smoke
{
    public sealed class ApiSmokeTests(PostgreSqlFixture fixture) : ApiTestBase(fixture)
    {
        [Fact]
        public async Task Host_starts_and_openapi_endpoint_is_available()
        {
            await using CustomWebApplicationFactory app = await CreateAppAsync();
            using HttpClient client = app.CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost"),
                AllowAutoRedirect = false,
            });

            HttpResponseMessage response = await client.GetAsync("/openapi/v1.json");

            response.EnsureSuccessStatusCode();
            string content = await response.Content.ReadAsStringAsync();

            Assert.Contains("\"openapi\"", content, StringComparison.OrdinalIgnoreCase);
        }
    }
}
