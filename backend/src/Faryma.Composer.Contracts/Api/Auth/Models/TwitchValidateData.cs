namespace Faryma.Composer.Contracts.Api.Auth.Contracts
{
    public sealed record TwitchValidateData(string ClientId, string Login, string UserId);
}