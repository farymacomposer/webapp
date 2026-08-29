using Faryma.Composer.Api.Test.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Faryma.Composer.Api.Test.Smoke
{
    public sealed class ApiSmokeTests : TestBase
    {
        [Fact]
        public async Task Host_starts_and_openapi_endpoint_is_available()
        {
            await using CustomWebApplicationFactory app = CreateApp();
            using HttpClient client = app.CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("http://localhost"),
                AllowAutoRedirect = false,
            });

            HttpResponseMessage response = await client.GetAsync("/openapi/v1.json", TestContext.Current.CancellationToken);

            response.EnsureSuccessStatusCode();
            string content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

            Assert.Contains("\"openapi\"", content, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Faryma.Composer.Api.Features.ReviewOrder.Complete.CompleteRequest", content, StringComparison.Ordinal);
            Assert.Contains("Faryma.Composer.Api.Features.ComposerStream.Complete.CompleteRequest", content, StringComparison.Ordinal);
        }
    }
}
