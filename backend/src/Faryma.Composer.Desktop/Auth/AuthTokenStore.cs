using System.Text.Json;

namespace Faryma.Composer.Desktop.Auth
{
    public sealed class AuthTokenStore(JsonSerializerOptions serializerOptions)
    {
        private static readonly string _sessionDirectoryPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Faryma.Composer.Desktop");

        private static readonly string _sessionFilePath = Path.Combine(_sessionDirectoryPath, "session.dat");

        public async Task<AuthTokens?> TryLoad()
        {
            if (!File.Exists(_sessionFilePath))
            {
                return null;
            }

            await using FileStream stream = File.OpenRead(_sessionFilePath);

            return await JsonSerializer.DeserializeAsync<AuthTokens>(stream, serializerOptions);
        }

        public async Task Save(AuthTokens tokens)
        {
            Directory.CreateDirectory(_sessionDirectoryPath);
            await using FileStream stream = File.Create(_sessionFilePath);
            await JsonSerializer.SerializeAsync(stream, tokens, serializerOptions);
        }

        public void Clear()
        {
            if (File.Exists(_sessionFilePath))
            {
                File.Delete(_sessionFilePath);
            }
        }
    }
}