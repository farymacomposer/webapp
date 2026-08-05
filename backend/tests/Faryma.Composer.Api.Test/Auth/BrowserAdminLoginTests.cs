using System.Net;
using System.Net.Http.Json;
using Faryma.Composer.Api.Contracts.Features.Auth.Login;
using Faryma.Composer.Api.Test.Infrastructure;
using Faryma.Composer.Api.Test.Infrastructure.Auth;
using Faryma.Composer.Domain;

namespace Faryma.Composer.Api.Test.Auth
{
    public sealed class BrowserAdminLoginTests(PostgreSqlFixture fixture) : DatabaseTestBase(fixture)
    {
        private const string _adminRoute = "/api/app-settings";
        private const string _browserAdminLoginRoute = "/api/auth/sessions/browser-admin";
        private const string _jwtLoginRoute = "/api/auth/sessions/desktop-admin";

        [Fact]
        public async Task Browser_admin_login_sets_cookie_and_allows_admin_endpoint()
        {
            await using CustomWebApplicationFactory app = await CreateAppAsync();
            SeededAuthUser admin = await SeedAdmin(app);
            using HttpClient client = app.CreateAnonymousClient();

            using HttpResponseMessage loginResponse = await Login(client, _browserAdminLoginRoute, admin);

            Assert.Equal(HttpStatusCode.NoContent, loginResponse.StatusCode);

            string authCookie = GetBrowserAuthCookie(loginResponse);
            using HttpRequestMessage adminRequest = new(HttpMethod.Get, _adminRoute);
            adminRequest.Headers.Add("Cookie", authCookie);

            using HttpResponseMessage adminResponse = await client.SendAsync(adminRequest);

            Assert.Equal(HttpStatusCode.OK, adminResponse.StatusCode);
        }

        [Fact]
        public async Task Browser_admin_login_rejects_non_admin_user()
        {
            await using CustomWebApplicationFactory app = await CreateAppAsync();
            SeededAuthUsers users = await app.SeedUsersAsync(new AuthTestSeedOptions
            {
                Browser = new TestAuthUserSeed
                {
                    UserName = "browser_password_user",
                    Password = "TestBrowserPass123!",
                    TwitchUserId = "browser-password-user-id",
                    TwitchLogin = "browser_password_user",
                },
            });
            using HttpClient client = app.CreateAnonymousClient();

            using HttpResponseMessage response = await Login(client, _browserAdminLoginRoute, users.Browser);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.False(response.Headers.TryGetValues("Set-Cookie", out _));
        }

        [Fact]
        public async Task Browser_admin_login_rejects_invalid_password()
        {
            await using CustomWebApplicationFactory app = await CreateAppAsync();
            SeededAuthUser admin = await SeedAdmin(app);
            using HttpClient client = app.CreateAnonymousClient();

            using HttpResponseMessage response = await client.PostAsJsonAsync(
                _browserAdminLoginRoute,
                new LoginRequest
                {
                    UserName = admin.UserName,
                    Password = "WrongPassword123!",
                });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.False(response.Headers.TryGetValues("Set-Cookie", out _));
        }

        [Fact]
        public async Task Jwt_login_still_returns_tokens_for_admin()
        {
            await using CustomWebApplicationFactory app = await CreateAppAsync();
            SeededAuthUser admin = await SeedAdmin(app);
            using HttpClient client = app.CreateAnonymousClient();

            using HttpResponseMessage response = await Login(client, _jwtLoginRoute, admin);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            LoginResponse login = await response.Content.ReadFromJsonAsync<LoginResponse>()
                ?? throw new InvalidOperationException("Ответ входа оказался пустым");
            Assert.False(string.IsNullOrWhiteSpace(login.AccessToken));
            Assert.False(string.IsNullOrWhiteSpace(login.RefreshToken));
        }

        private static async Task<SeededAuthUser> SeedAdmin(CustomWebApplicationFactory app)
        {
            SeededAuthUsers users = await app.SeedUsersAsync(new AuthTestSeedOptions
            {
                Admin = new TestAuthUserSeed
                {
                    UserName = "browser_admin_login_composer",
                    Password = "TestComposerPass123!",
                    Roles = [AppRoles.Composer],
                },
            });

            return users.Admin;
        }

        private static Task<HttpResponseMessage> Login(HttpClient client, string route, SeededAuthUser user)
        {
            return client.PostAsJsonAsync(
                route,
                new LoginRequest
                {
                    UserName = user.UserName,
                    Password = user.Password ?? throw new InvalidOperationException("У тестового пользователя нет пароля"),
                });
        }

        private static string GetBrowserAuthCookie(HttpResponseMessage response)
        {
            Assert.True(response.Headers.TryGetValues("Set-Cookie", out IEnumerable<string>? cookies));

            string cookie = Assert.Single(
                cookies,
                value => value.StartsWith("faryma_browser_auth=", StringComparison.Ordinal));

            return cookie.Split(';', 2)[0];
        }
    }
}
