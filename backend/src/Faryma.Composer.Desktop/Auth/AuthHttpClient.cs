using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Faryma.Composer.Contracts.Api.Auth.Features.Login;
using Faryma.Composer.Contracts.Api.Auth.Features.Logout;
using Faryma.Composer.Contracts.Api.Auth.Features.RefreshToken;

namespace Faryma.Composer.Desktop.Auth
{
    public sealed class AuthHttpClient(HttpClient httpClient, JsonSerializerOptions serializerOptions)
    {
        public async Task<LoginResponse> Login(string userName, string password, CancellationToken ct = default)
        {
            HttpResponseMessage response = await httpClient.PostAsJsonAsync("/api/Auth/Login", new LoginRequest
            {
                UserName = userName,
                Password = password,
            }, serializerOptions, ct);

            await EnsureSuccessStatusCode(response);

            return await response.Content.ReadFromJsonAsync<LoginResponse>(serializerOptions, ct)
                ?? throw new InvalidOperationException("Не удалось десериализовать LoginResponse");
        }

        public async Task<RefreshTokenResponse> Refresh(string refreshToken, CancellationToken ct = default)
        {
            HttpResponseMessage response = await httpClient.PostAsJsonAsync("/api/Auth/RefreshToken", new RefreshTokenRequest
            {
                RefreshToken = refreshToken
            }, serializerOptions, ct);

            await EnsureSuccessStatusCode(response);

            return await response.Content.ReadFromJsonAsync<RefreshTokenResponse>(serializerOptions, ct)
                ?? throw new InvalidOperationException("Не удалось десериализовать RefreshTokenResponse");
        }

        public async Task Logout(string refreshToken, string accessToken, CancellationToken ct = default)
        {
            using HttpRequestMessage request = new(HttpMethod.Post, "/api/Auth/Logout")
            {
                Content = JsonContent.Create(new LogoutRequest
                {
                    RefreshToken = refreshToken
                }, options: serializerOptions)
            };

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            HttpResponseMessage response = await httpClient.SendAsync(request, ct);

            await EnsureSuccessStatusCode(response);
        }

        private static async Task EnsureSuccessStatusCode(HttpResponseMessage response)
        {
            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new InvalidOperationException("Неверное имя пользователя или пароль.", ex);
            }
        }
    }
}