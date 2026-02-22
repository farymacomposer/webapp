namespace Faryma.Composer.Api.Auth
{
    public interface ITwitchTokenValidationClient
    {
        Task<TwitchValidateData> ValidateAccessToken(string accessToken, CancellationToken cancellationToken);
    }

    public sealed record TwitchValidateData(string ClientId, string Login, string UserId);
}