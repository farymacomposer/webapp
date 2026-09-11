using System.Net;
using System.Net.Http.Json;
using Faryma.Composer.Api.Features.Auth.BrowserAdminLogin;
using Faryma.Composer.Api.Features.Auth.DesktopAdminLogin;
using Faryma.Composer.Api.Features.Auth.Dtos;
using Faryma.Composer.Api.Test.Infrastructure;
using Faryma.Composer.Api.Test.Infrastructure.Auth;
using Faryma.Composer.Domain;

namespace Faryma.Composer.Api.Test.Features.Auth
{
    [TestClass]
    [DoNotParallelize]
    public sealed class BrowserAdminLoginTests : DatabaseTestBase
    {
        private const string _adminRoute = "/api/AppSettings/Get";
        private const string _browserAdminLoginRoute = "/api/Auth/BrowserAdminLogin";
        private const string _jwtLoginRoute = "/api/Auth/DesktopAdminLogin";

        [TestMethod]
        public async Task Browser_admin_login_sets_cookie_and_allows_admin_endpoint()
        {
            await using CustomWebApplicationFactory app = await CreateAppAsync();
            SeededAuthUser admin = await SeedAdmin(app);
            using HttpClient client = app.CreateAnonymousClient();

            using HttpResponseMessage loginResponse = await Login(client, _browserAdminLoginRoute, admin);

            Assert.AreEqual(HttpStatusCode.NoContent, loginResponse.StatusCode);

            string authCookie = GetBrowserAuthCookie(loginResponse);
            using HttpRequestMessage adminRequest = new(HttpMethod.Get, _adminRoute);
            adminRequest.Headers.Add("Cookie", authCookie);

            using HttpResponseMessage adminResponse = await client.SendAsync(adminRequest, TestContext.CancellationToken);

            Assert.AreEqual(HttpStatusCode.OK, adminResponse.StatusCode);
        }

        [TestMethod]
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
            }, ct: TestContext.CancellationToken);
            using HttpClient client = app.CreateAnonymousClient();

            using HttpResponseMessage response = await Login(client, _browserAdminLoginRoute, users.Browser);

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.IsFalse(response.Headers.TryGetValues("Set-Cookie", out _));
        }

        [TestMethod]
        public async Task Browser_admin_login_rejects_invalid_password()
        {
            await using CustomWebApplicationFactory app = await CreateAppAsync();
            SeededAuthUser admin = await SeedAdmin(app);
            using HttpClient client = app.CreateAnonymousClient();

            using HttpResponseMessage response = await client.PostAsJsonAsync(_browserAdminLoginRoute, new BrowserAdminLoginRequest
            {
                Credentials = new AdminCredentialsDto
                {
                    UserName = admin.UserName,
                    Password = "WrongPassword123!",
                }
            }, cancellationToken: TestContext.CancellationToken);

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.IsFalse(response.Headers.TryGetValues("Set-Cookie", out _));
        }

        [TestMethod]
        public async Task Jwt_login_still_returns_tokens_for_admin()
        {
            await using CustomWebApplicationFactory app = await CreateAppAsync();
            SeededAuthUser admin = await SeedAdmin(app);
            using HttpClient client = app.CreateAnonymousClient();

            using HttpResponseMessage response = await Login(client, _jwtLoginRoute, admin);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            DesktopAdminLoginResponse login = await response.Content.ReadFromJsonAsync<DesktopAdminLoginResponse>(cancellationToken: TestContext.CancellationToken) ?? throw new InvalidOperationException("Ответ входа оказался пустым");
            Assert.IsFalse(string.IsNullOrWhiteSpace(login.Tokens.AccessToken));
            Assert.IsFalse(string.IsNullOrWhiteSpace(login.Tokens.RefreshToken));
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
                new BrowserAdminLoginRequest
                {
                    Credentials = new AdminCredentialsDto
                    {
                        UserName = user.UserName,
                        Password = user.Password ?? throw new InvalidOperationException("У тестового пользователя нет пароля"),
                    }
                });
        }

        private static string GetBrowserAuthCookie(HttpResponseMessage response)
        {
            Assert.IsTrue(response.Headers.TryGetValues("Set-Cookie", out IEnumerable<string>? cookies));
            Assert.IsNotNull(cookies);

            string cookie = Assert.ContainsSingle(
                cookies!.Where(value => value.StartsWith("faryma_browser_auth=", StringComparison.Ordinal)));

            return cookie.Split(';', 2)[0];
        }
    }
}
