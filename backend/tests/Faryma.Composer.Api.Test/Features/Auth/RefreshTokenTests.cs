using System.Data.Common;
using System.Net;
using System.Net.Http.Json;
using Faryma.Composer.Api.Features.Auth.DesktopAdminLogin;
using Faryma.Composer.Api.Features.Auth.Dtos;
using Faryma.Composer.Api.Features.Auth.RefreshToken;
using Faryma.Composer.Api.Test.Infrastructure;
using Faryma.Composer.Api.Test.Infrastructure.Auth;
using Faryma.Composer.Domain;
using Faryma.Composer.Domain.Entities;
using Faryma.Composer.Infrastructure;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace Faryma.Composer.Api.Test.Features.Auth
{
    [TestClass]
    [DoNotParallelize]
    public sealed class RefreshTokenTests : DatabaseTestBase
    {
        private const string _loginRoute = "/api/Auth/DesktopAdminLogin";
        private const string _refreshRoute = "/api/Auth/RefreshToken";

        [TestMethod]
        public async Task Refresh_rotates_token_and_returns_new_usable_token()
        {
            await using CustomWebApplicationFactory app = await CreateAppAsync();
            SeededAuthUser admin = await SeedAdmin(app);
            using HttpClient client = app.CreateAnonymousClient();
            AuthTokensDto initialTokens = await Login(client, admin);

            using HttpResponseMessage firstResponse = await Refresh(client, initialTokens.RefreshToken);
            Assert.AreEqual(HttpStatusCode.OK, firstResponse.StatusCode);
            AuthTokensDto rotatedTokens = await ReadTokens(firstResponse);

            using HttpResponseMessage secondResponse = await Refresh(client, rotatedTokens.RefreshToken);
            Assert.AreEqual(HttpStatusCode.OK, secondResponse.StatusCode);
        }

        [TestMethod]
        public async Task Sequential_refresh_replay_revokes_token_family()
        {
            await using CustomWebApplicationFactory app = await CreateAppAsync();
            SeededAuthUser admin = await SeedAdmin(app);
            using HttpClient client = app.CreateAnonymousClient();
            AuthTokensDto initialTokens = await Login(client, admin);

            using HttpResponseMessage rotationResponse = await Refresh(client, initialTokens.RefreshToken);
            Assert.AreEqual(HttpStatusCode.OK, rotationResponse.StatusCode);
            AuthTokensDto rotatedTokens = await ReadTokens(rotationResponse);

            using HttpResponseMessage replayResponse = await Refresh(client, initialTokens.RefreshToken);
            Assert.AreEqual(HttpStatusCode.Unauthorized, replayResponse.StatusCode);

            using HttpResponseMessage familyResponse = await Refresh(client, rotatedTokens.RefreshToken);
            Assert.AreEqual(HttpStatusCode.Unauthorized, familyResponse.StatusCode);
        }

        [TestMethod]
        public async Task Concurrent_refresh_replay_revokes_family_and_returns_unauthorized()
        {
            await using CustomWebApplicationFactory app = await CreateAppAsync();
            var barrier = new RefreshTokenReadBarrierInterceptor();
            CustomWebApplicationFactory concurrentApp = app.CreateDerivedFactory(builder =>
            {
                builder.ConfigureTestServices(services =>
                    services.AddDbContextFactory<AppDbContext>((_, options) => options.AddInterceptors(barrier)));
            });
            SeededAuthUser admin = await SeedAdmin(concurrentApp);
            using HttpClient client = concurrentApp.CreateAnonymousClient();
            AuthTokensDto tokens = await Login(client, admin);
            barrier.Arm();

            Task<HttpResponseMessage> firstTask = Refresh(client, tokens.RefreshToken);
            Task<HttpResponseMessage> secondTask = Refresh(client, tokens.RefreshToken);
            HttpResponseMessage[] responses = await Task.WhenAll(firstTask, secondTask);

            using HttpResponseMessage firstResponse = responses[0];
            using HttpResponseMessage secondResponse = responses[1];
            Assert.IsTrue(responses.Any(response => response.StatusCode == HttpStatusCode.OK));
            Assert.IsTrue(responses.Any(response => response.StatusCode == HttpStatusCode.Unauthorized));

            HttpResponseMessage successfulResponse = responses.Single(response => response.StatusCode == HttpStatusCode.OK);
            AuthTokensDto successfulTokens = await ReadTokens(successfulResponse);

            using HttpResponseMessage familyResponse = await Refresh(client, successfulTokens.RefreshToken);
            Assert.AreEqual(HttpStatusCode.Unauthorized, familyResponse.StatusCode);
        }

        [TestMethod]
        public async Task Concurrent_expired_refresh_returns_unauthorized_for_both_requests()
        {
            await using CustomWebApplicationFactory app = await CreateAppAsync();
            var barrier = new RefreshTokenReadBarrierInterceptor();
            CustomWebApplicationFactory concurrentApp = app.CreateDerivedFactory(builder =>
            {
                builder.ConfigureTestServices(services =>
                    services.AddDbContextFactory<AppDbContext>((_, options) => options.AddInterceptors(barrier)));
            });
            SeededAuthUser admin = await SeedAdmin(concurrentApp);
            using HttpClient client = concurrentApp.CreateAnonymousClient();
            AuthTokensDto tokens = await Login(client, admin);

            await concurrentApp.Services.RunInScopeAsync(async scoped =>
            {
                AppDbContext appDbContext = scoped.GetRequiredService<AppDbContext>();
                RefreshTokenEntity stored = await appDbContext.RefreshTokens.SingleAsync(
                    token => token.UserId == admin.UserId,
                    TestContext.CancellationToken);
                stored.ExpiresAt = DateTime.UtcNow.AddMinutes(-1);
                await appDbContext.SaveChangesAsync(TestContext.CancellationToken);
            });

            barrier.Arm();
            Task<HttpResponseMessage> firstTask = Refresh(client, tokens.RefreshToken);
            Task<HttpResponseMessage> secondTask = Refresh(client, tokens.RefreshToken);
            HttpResponseMessage[] responses = await Task.WhenAll(firstTask, secondTask);

            using HttpResponseMessage firstResponse = responses[0];
            using HttpResponseMessage secondResponse = responses[1];
            foreach (HttpResponseMessage response in responses)
            {
                Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
            }
        }

        private async Task<SeededAuthUser> SeedAdmin(CustomWebApplicationFactory app)
        {
            SeededAuthUsers users = await app.SeedUsersAsync(new AuthTestSeedOptions
            {
                Admin = new TestAuthUserSeed
                {
                    UserName = "refresh_token_composer",
                    Password = "TestComposerPass123!",
                    Roles = [AppRoles.Composer],
                },
            }, ct: TestContext.CancellationToken);

            return users.Admin;
        }

        private async Task<AuthTokensDto> Login(HttpClient client, SeededAuthUser admin)
        {
            using HttpResponseMessage response = await client.PostAsJsonAsync(
                _loginRoute,
                new DesktopAdminLoginRequest
                {
                    Credentials = new AdminCredentialsDto
                    {
                        UserName = admin.UserName,
                        Password = admin.Password ?? throw new InvalidOperationException("У тестового пользователя нет пароля"),
                    }
                },
                TestContext.CancellationToken);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            DesktopAdminLoginResponse? login =
                await response.Content.ReadFromJsonAsync<DesktopAdminLoginResponse>(cancellationToken: TestContext.CancellationToken);

            return login?.Tokens ?? throw new InvalidOperationException("Ответ входа оказался пустым");
        }

        private async Task<AuthTokensDto> ReadTokens(HttpResponseMessage response)
        {
            RefreshTokenResponse? refresh =
                await response.Content.ReadFromJsonAsync<RefreshTokenResponse>(cancellationToken: TestContext.CancellationToken);

            return refresh?.Tokens ?? throw new InvalidOperationException("Ответ обновления токенов оказался пустым");
        }

        private Task<HttpResponseMessage> Refresh(HttpClient client, string refreshToken)
        {
            return client.PostAsJsonAsync(
                _refreshRoute,
                new RefreshTokenRequest { RefreshToken = refreshToken },
                TestContext.CancellationToken);
        }

        private sealed class RefreshTokenReadBarrierInterceptor : DbCommandInterceptor
        {
            private TaskCompletionSource _bothReadsCompleted = CreateCompletionSource();
            private int _readCount;
            private int _armed;

            public void Arm()
            {
                _bothReadsCompleted = CreateCompletionSource();
                _readCount = 0;
                Volatile.Write(ref _armed, 1);
            }

            public override async ValueTask<DbDataReader> ReaderExecutedAsync(
                DbCommand command,
                CommandExecutedEventData eventData,
                DbDataReader result,
                CancellationToken cancellationToken = default)
            {
                if (Volatile.Read(ref _armed) == 0
                    || !command.CommandText.Contains("refresh_tokens", StringComparison.Ordinal))
                {
                    return result;
                }

                int readNumber = Interlocked.Increment(ref _readCount);
                if (readNumber > 2)
                {
                    return result;
                }

                if (readNumber == 2)
                {
                    Volatile.Write(ref _armed, 0);
                    _bothReadsCompleted.TrySetResult();
                }

                await _bothReadsCompleted.Task.WaitAsync(TimeSpan.FromSeconds(10), cancellationToken);
                return result;
            }

            private static TaskCompletionSource CreateCompletionSource() =>
                new(TaskCreationOptions.RunContinuationsAsynchronously);
        }
    }
}
