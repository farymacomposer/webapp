namespace Faryma.Composer.Contracts.Api.Auth.Contracts
{
    public interface ITwitchTokenValidationClient
    {
        Task<TwitchValidateData> ValidateAccessToken(string accessToken, CancellationToken cancellationToken);
    }
}