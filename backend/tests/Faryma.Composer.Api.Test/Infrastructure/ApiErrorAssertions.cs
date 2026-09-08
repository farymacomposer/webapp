using System.Text.Json;

namespace Faryma.Composer.Api.Test.Infrastructure
{
    public static class ApiErrorAssertions
    {
        public static async Task<string?> AssertApiErrorAsync(
            this HttpResponseMessage response,
            string code)
        {
            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
            Assert.Equal($"urn:composer:error:{code}", json.RootElement.GetProperty("type").GetString());

            return json.RootElement.GetProperty("detail").GetString();
        }
    }
}
