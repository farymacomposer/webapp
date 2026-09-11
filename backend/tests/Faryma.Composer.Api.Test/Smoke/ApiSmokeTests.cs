using Faryma.Composer.Api.Test.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Faryma.Composer.Api.Test.Smoke
{
    [TestClass]
    public sealed class ApiSmokeTests : TestBase
    {
        [TestMethod]
        public async Task Host_starts_and_openapi_endpoint_is_available()
        {
            await using CustomWebApplicationFactory app = CreateApp();
            using HttpClient client = app.CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("http://localhost"),
                AllowAutoRedirect = false,
            });

            HttpResponseMessage response = await client.GetAsync("/openapi/v1.json", TestContext.CancellationToken);

            response.EnsureSuccessStatusCode();
            string content = await response.Content.ReadAsStringAsync(TestContext.CancellationToken);

            Assert.IsTrue(content.Contains("\"openapi\"", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(content.Contains("Faryma.Composer.Api.Features.ReviewOrder.Complete.CompleteRequest", StringComparison.Ordinal));
            Assert.IsTrue(content.Contains("Faryma.Composer.Api.Features.ComposerStream.Complete.CompleteRequest", StringComparison.Ordinal));
        }
    }
}
