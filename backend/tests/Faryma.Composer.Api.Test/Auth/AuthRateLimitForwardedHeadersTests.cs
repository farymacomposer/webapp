using System.Net;
using System.Net.Http.Json;
using Faryma.Composer.Api.Test.Infrastructure;
using Faryma.Composer.Contracts.Api.Features.Auth.Login;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Faryma.Composer.Api.Test.Auth
{
    public sealed class AuthRateLimitForwardedHeadersTests(PostgreSqlFixture fixture) : TestBase(fixture)
    {
        private const int _loginPermitLimit = 10;
        private const string _jwtLoginRoute = "/api/auth/sessions/desktop-admin";

        [Fact]
        public async Task Trusted_forwarded_for_partitions_auth_login_rate_limit_by_client_ip()
        {
            await using CustomWebApplicationFactory app = await CreateAppAsync();
            using HttpClient client = CreateForwardedHeadersClient(
                app,
                IPAddress.Parse("192.0.2.10"),
                new Dictionary<string, string?>
                {
                    ["FORWARDED_HEADERS:KNOWN_NETWORKS:0"] = "192.0.2.0/24"
                });

            for (int i = 0; i < _loginPermitLimit; i++)
            {
                using HttpResponseMessage response = await PostInvalidLogin(client, "203.0.113.10");

                Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            }

            using HttpResponseMessage otherClientResponse = await PostInvalidLogin(client, "203.0.113.11");
            Assert.Equal(HttpStatusCode.Unauthorized, otherClientResponse.StatusCode);

            using HttpResponseMessage limitedResponse = await PostInvalidLogin(client, "203.0.113.10");
            Assert.Equal(HttpStatusCode.TooManyRequests, limitedResponse.StatusCode);
        }

        [Fact]
        public async Task Untrusted_forwarded_for_does_not_partition_auth_login_rate_limit()
        {
            await using CustomWebApplicationFactory app = await CreateAppAsync();
            using HttpClient client = CreateForwardedHeadersClient(
                app,
                IPAddress.Parse("192.0.2.10"),
                new Dictionary<string, string?>
                {
                    ["FORWARDED_HEADERS:KNOWN_PROXIES:0"] = "10.0.0.10"
                });

            for (int i = 0; i < _loginPermitLimit; i++)
            {
                using HttpResponseMessage response = await PostInvalidLogin(client, "203.0.113.10");

                Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            }

            using HttpResponseMessage limitedResponse = await PostInvalidLogin(client, "203.0.113.11");
            Assert.Equal(HttpStatusCode.TooManyRequests, limitedResponse.StatusCode);
        }

        private static HttpClient CreateForwardedHeadersClient(
            CustomWebApplicationFactory app,
            IPAddress remoteIpAddress,
            IReadOnlyDictionary<string, string?> configuration)
        {
            CustomWebApplicationFactory forwardedHeadersApp = app.CreateDerivedFactory(builder =>
            {
                builder.ConfigureAppConfiguration((_, configurationBuilder) =>
                    configurationBuilder.AddInMemoryCollection(configuration));

                builder.ConfigureTestServices(services =>
                    services.AddSingleton<IStartupFilter>(new RemoteIpAddressStartupFilter(remoteIpAddress)));
            });

            return forwardedHeadersApp.CreateAnonymousClient();
        }

        private static async Task<HttpResponseMessage> PostInvalidLogin(HttpClient client, string forwardedFor)
        {
            using HttpRequestMessage request = new(HttpMethod.Post, _jwtLoginRoute);
            request.Headers.Add("X-Forwarded-For", forwardedFor);
            request.Headers.Add("X-Forwarded-Proto", "https");
            request.Content = JsonContent.Create(new LoginRequest
            {
                UserName = "missing_admin",
                Password = "WrongPassword123!",
            });

            return await client.SendAsync(request);
        }

        private sealed class RemoteIpAddressStartupFilter(IPAddress remoteIpAddress) : IStartupFilter
        {
            public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
            {
                return app =>
                {
                    app.Use((context, nextMiddleware) =>
                    {
                        context.Connection.RemoteIpAddress = remoteIpAddress;
                        return nextMiddleware(context);
                    });

                    next(app);
                };
            }
        }
    }
}
