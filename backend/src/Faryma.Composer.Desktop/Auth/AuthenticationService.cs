using System.Text.Json;
using Faryma.Composer.Contracts.Api.Features.Auth.Login;
using Faryma.Composer.Contracts.Api.Features.Auth.RefreshToken;

namespace Faryma.Composer.Desktop.Auth
{
    public sealed class AuthenticationService(AuthHttpClient authHttpClient, AuthTokenStore authTokenStore)
    {
        private static readonly TimeSpan _accessTokenRefreshThreshold = TimeSpan.FromMinutes(1);
        private readonly SemaphoreSlim _refreshLock = new(1, 1);
        private AuthTokens? _tokens;

        public bool IsAuthenticated => _tokens is not null;

        public async Task<bool> TryRestoreSession()
        {
            AuthTokens? storedTokens = await authTokenStore.TryLoad();
            if (storedTokens is null)
            {
                return false;
            }

            _tokens = storedTokens;

            return await TryRefreshInternal();
        }

        public async Task Login(string userName, string password)
        {
            string normalizedUserName = userName.Trim();

            AuthTokens tokens = await Exchange(async () =>
            {
                LoginResponse response = await authHttpClient.Login(normalizedUserName, password);
                return new AuthTokens
                {
                    AccessToken = response.AccessToken,
                    RefreshToken = response.RefreshToken,
                };
            });

            _tokens = tokens;
        }

        public async Task<string?> GetAccessToken(CancellationToken ct)
        {
            if (_tokens is null)
            {
                return null;
            }

            if (ShouldRefresh(_tokens.AccessToken))
            {
                bool refreshed = await TryRefreshInternal(ct);
                if (!refreshed)
                {
                    return null;
                }
            }

            return _tokens?.AccessToken;
        }

        public async Task Logout()
        {
            if (_tokens is not null)
            {
                try
                {
                    await authHttpClient.Logout(_tokens.RefreshToken, _tokens.AccessToken);
                }
                catch
                {
                    // Local cleanup is more important than surfacing logout failures here.
                }
            }

            ClearSession();
        }

        private static bool ShouldRefresh(string accessToken)
        {
            DateTimeOffset expiresAt = GetExpiresAt(accessToken);

            return expiresAt <= DateTimeOffset.UtcNow.Add(_accessTokenRefreshThreshold);
        }

        private static DateTimeOffset GetExpiresAt(string accessToken)
        {
            string[] segments = accessToken.Split('.');
            if (segments.Length < 2)
            {
                throw new InvalidOperationException("Некорректный access token");
            }

            string payload = segments[1]
                .Replace('-', '+')
                .Replace('_', '/');

            int remainder = payload.Length % 4;
            if (remainder > 0)
            {
                payload = payload.PadRight(payload.Length + (4 - remainder), '=');
            }

            byte[] bytes = Convert.FromBase64String(payload);
            using var document = JsonDocument.Parse(bytes);

            long expiresAtUnixTime = document.RootElement
                .GetProperty("exp")
                .GetInt64();

            return DateTimeOffset.FromUnixTimeSeconds(expiresAtUnixTime);
        }

        private async Task<bool> TryRefreshInternal(CancellationToken ct = default)
        {
            if (_tokens is null)
            {
                return false;
            }

            await _refreshLock.WaitAsync(ct);
            try
            {
                if (_tokens is null)
                {
                    return false;
                }

                if (!ShouldRefresh(_tokens.AccessToken))
                {
                    return true;
                }

                try
                {
                    AuthTokens refreshedTokens = await Exchange(async () =>
                    {
                        RefreshTokenResponse response = await authHttpClient.RefreshToken(_tokens.RefreshToken, ct);

                        return new AuthTokens
                        {
                            AccessToken = response.AccessToken,
                            RefreshToken = response.RefreshToken,
                        };
                    });

                    _tokens = refreshedTokens;

                    return true;
                }
                catch
                {
                    ClearSession();

                    return false;
                }
            }
            finally
            {
                _refreshLock.Release();
            }
        }

        private async Task<AuthTokens> Exchange(Func<Task<AuthTokens>> exchange)
        {
            AuthTokens tokens = await exchange();
            await authTokenStore.Save(tokens);

            return tokens;
        }

        private void ClearSession()
        {
            _tokens = null;
            authTokenStore.Clear();
        }
    }
}
