using System.Security.Cryptography;
using Microsoft.Extensions.Caching.Memory;

namespace Faryma.Composer.Api.Auth
{
    public sealed class TwitchOAuthStateService(IMemoryCache memoryCache)
    {
        private static readonly TimeSpan _stateLifetime = TimeSpan.FromMinutes(10);
        private const string _stateKeyPrefix = "auth:twitch:state:";

        public string IssueState()
        {
            string state = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
            memoryCache.Set(BuildCacheKey(state), true, _stateLifetime);

            return state;
        }

        public bool TryConsumeState(string state)
        {
            if (string.IsNullOrWhiteSpace(state))
            {
                return false;
            }

            string cacheKey = BuildCacheKey(state);
            if (!memoryCache.TryGetValue(cacheKey, out _))
            {
                return false;
            }

            memoryCache.Remove(cacheKey);
            return true;
        }

        private static string BuildCacheKey(string state) => _stateKeyPrefix + state;
    }
}
