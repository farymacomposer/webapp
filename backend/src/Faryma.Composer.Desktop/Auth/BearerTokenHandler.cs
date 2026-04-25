using System.Net.Http.Headers;

namespace Faryma.Composer.Desktop.Auth
{
    public sealed class BearerTokenHandler(AuthenticationService authenticationService) : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string? accessToken = await authenticationService.GetAccessToken(cancellationToken);
            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
