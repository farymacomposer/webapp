namespace Faryma.Composer.Api.Auth
{
    public interface ITwitchPkceCodeExchangeClient
    {
        Task<string> ExchangeCodeWithPkce(string code, string codeVerifier, CancellationToken cancellationToken);
    }
}