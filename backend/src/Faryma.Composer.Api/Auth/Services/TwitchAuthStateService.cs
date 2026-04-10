using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Caching.Memory;

namespace Faryma.Composer.Api.Auth.Services
{
    public sealed class TwitchAuthStateService(IMemoryCache memoryCache)
    {
        /// <summary>
        /// Имя cookie для хранения nonce браузера
        /// </summary>
        public const string BrowserNonceCookieName = "faryma_auth_twitch_nonce";

        /// <summary>
        /// Время жизни nonce браузера
        /// </summary>
        public static readonly TimeSpan StateLifetime = TimeSpan.FromMinutes(10);

        public (string State, string BrowserNonce) IssueState()
        {
            string state = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
            string browserNonce = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
            byte[] nonceHash = SHA256.HashData(Encoding.UTF8.GetBytes(browserNonce));
            memoryCache.Set(BuildCacheKey(state), nonceHash, StateLifetime);

            return (state, browserNonce);
        }

        public bool TryConsumeState(string state, string? browserNonce)
        {
            if (string.IsNullOrWhiteSpace(state) || string.IsNullOrWhiteSpace(browserNonce))
            {
                return false;
            }

            string cacheKey = BuildCacheKey(state);
            if (!memoryCache.TryGetValue(cacheKey, out byte[]? expectedNonceHash) || expectedNonceHash is null)
            {
                return false;
            }

            byte[] actualNonceHash = SHA256.HashData(Encoding.UTF8.GetBytes(browserNonce));
            if (!CryptographicOperations.FixedTimeEquals(expectedNonceHash, actualNonceHash))
            {
                return false;
            }

            memoryCache.Remove(cacheKey);

            return true;
        }

        private static string BuildCacheKey(string state) => "auth:twitch:state:" + state;
    }
}