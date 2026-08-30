using System.Text.Json;

namespace Faryma.Composer.Api.Test.Infrastructure
{
    public static class ApiErrorAssertions
    {
        public static async Task<string?> AssertApiErrorAsync(
            this HttpResponseMessage response,
            string exceptionType)
        {
            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
            Assert.Equal(exceptionType, json.RootElement.GetProperty("ExceptionType").GetString());

            return json.RootElement.GetProperty("Message").GetString();
        }
    }
}
