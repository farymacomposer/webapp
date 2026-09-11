using System.Net;
using System.Text.Json;
using Faryma.Composer.Api.Test.Infrastructure;

namespace Faryma.Composer.Api.Test.Common
{
    [TestClass]
    public sealed class ApiExceptionHandlerTests : TestBase
    {
        [TestMethod]
        public async Task AppException_IsMappedToProblemDetails()
        {
            await using CustomWebApplicationFactory app = CreateApp();
            using HttpClient client = app.CreateAnonymousClient();

            using HttpResponseMessage response = await client.GetAsync("/api/_test/exceptions/app", TestContext.CancellationToken);

            Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode);
            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.CancellationToken));
            Assert.AreEqual("urn:composer:error:test_api", json.RootElement.GetProperty("type").GetString());
            Assert.AreEqual("Ошибка бизнес-логики", json.RootElement.GetProperty("title").GetString());
            Assert.AreEqual(409, json.RootElement.GetProperty("status").GetInt32());
            Assert.AreEqual("Тестовая ошибка API", json.RootElement.GetProperty("detail").GetString());
        }

        [TestMethod]
        public async Task AuthenticationException_IsMappedToUnauthorizedPayload()
        {
            await using CustomWebApplicationFactory app = CreateApp();
            using HttpClient client = app.CreateAnonymousClient();

            using HttpResponseMessage response = await client.GetAsync("/api/_test/exceptions/authentication", TestContext.CancellationToken);

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.CancellationToken));
            Assert.AreEqual("Ошибка аутентификации", json.RootElement.GetProperty("Message").GetString());
        }

        [TestMethod]
        public async Task UnhandledException_IsMappedToProblemDetails()
        {
            await using CustomWebApplicationFactory app = CreateApp();
            using HttpClient client = app.CreateAnonymousClient();

            using HttpResponseMessage response = await client.GetAsync("/api/_test/exceptions/unhandled", TestContext.CancellationToken);

            Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.CancellationToken));
            Assert.AreEqual("Произошла непредвиденная ошибка", json.RootElement.GetProperty("title").GetString());
            Assert.AreEqual(500, json.RootElement.GetProperty("status").GetInt32());
        }
    }
}
