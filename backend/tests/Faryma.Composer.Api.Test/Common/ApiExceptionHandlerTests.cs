using System.Net;
using System.Text.Json;
using Faryma.Composer.Api.Test.Infrastructure;
using Faryma.Composer.Domain.Exceptions;

namespace Faryma.Composer.Api.Test.Common
{
    public sealed class ApiExceptionHandlerTests : TestBase
    {
        [Fact]
        public async Task AppException_IsMappedToLegacyApiErrorPayload()
        {
            await using CustomWebApplicationFactory app = CreateApp();
            using HttpClient client = app.CreateAnonymousClient();

            using HttpResponseMessage response = await client.GetAsync("/api/_test/exceptions/app");

            Assert.Equal(AppException.StatusCode, (int)response.StatusCode);
            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            Assert.Equal(nameof(TestApiException), json.RootElement.GetProperty("ExceptionType").GetString());
            Assert.Equal("Тестовая ошибка API", json.RootElement.GetProperty("Message").GetString());
        }

        [Fact]
        public async Task AuthenticationException_IsMappedToUnauthorizedPayload()
        {
            await using CustomWebApplicationFactory app = CreateApp();
            using HttpClient client = app.CreateAnonymousClient();

            using HttpResponseMessage response = await client.GetAsync("/api/_test/exceptions/authentication");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            Assert.Equal("Ошибка аутентификации", json.RootElement.GetProperty("Message").GetString());
        }

        [Fact]
        public async Task UnhandledException_IsMappedToProblemDetails()
        {
            await using CustomWebApplicationFactory app = CreateApp();
            using HttpClient client = app.CreateAnonymousClient();

            using HttpResponseMessage response = await client.GetAsync("/api/_test/exceptions/unhandled");

            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            Assert.Equal("Произошла непредвиденная ошибка", json.RootElement.GetProperty("title").GetString());
            Assert.Equal(500, json.RootElement.GetProperty("status").GetInt32());
        }
    }
}
