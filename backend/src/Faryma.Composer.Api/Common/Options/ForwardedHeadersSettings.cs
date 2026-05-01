using System.Globalization;
using System.Net;

namespace Faryma.Composer.Api.Common.Options
{
    public sealed record ForwardedHeadersSettings
    {
        [ConfigurationKeyName("KNOWN_PROXIES")]
        public string[] KnownProxies { get; init; } = [];

        [ConfigurationKeyName("KNOWN_NETWORKS")]
        public string[] KnownNetworks { get; init; } = [];

        public bool HasTrustedForwarders => KnownProxies.Length > 0 || KnownNetworks.Length > 0;

        public static bool HasValidKnownProxies(ForwardedHeadersSettings settings) =>
            settings.KnownProxies.All(value => IPAddress.TryParse(value, out _));

        public static bool HasValidKnownNetworks(ForwardedHeadersSettings settings) =>
            settings.KnownNetworks.All(CanParseNetwork);

        public static IPNetwork ParseKnownNetwork(string value)
        {
            string[] parts = value.Split('/', 2, StringSplitOptions.TrimEntries);

            return new IPNetwork(
                IPAddress.Parse(parts[0]),
                int.Parse(parts[1], CultureInfo.InvariantCulture));
        }

        private static bool CanParseNetwork(string value)
        {
            string[] parts = value.Split('/', 2, StringSplitOptions.TrimEntries);
            if (parts.Length != 2 || !IPAddress.TryParse(parts[0], out IPAddress? address))
            {
                return false;
            }

            if (!int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out int prefixLength))
            {
                return false;
            }

            int maxPrefixLength = address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork
                ? 32
                : 128;

            return prefixLength >= 0 && prefixLength <= maxPrefixLength;
        }
    }
}
