namespace Faryma.Composer.Contracts.Api.Auth.Contracts
{
    public interface ITwitchPkceCodeExchangeClient
    {
        Task<string> ExchangeCodeWithPkce(string code, string codeVerifier, CancellationToken cancellationToken);
    }
}