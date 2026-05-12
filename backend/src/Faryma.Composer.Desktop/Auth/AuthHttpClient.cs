using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Faryma.Composer.Contracts.Api.Features.Auth.Login;
using Faryma.Composer.Contracts.Api.Features.Auth.Logout;
using Faryma.Composer.Contracts.Api.Features.Auth.RefreshToken;

namespace Faryma.Composer.Desktop.Auth
{
    public sealed class AuthHttpClient(HttpClient httpClient, JsonSerializerOptions serializerOptions)
    {
        public Task<LoginResponse> Login(string userName, string password)
        {
            return Post<LoginRequest, LoginResponse>(
                "/api/auth/sessions/desktop-admin",
                new LoginRequest
                {
                    UserName = userName,
                    Password = password,
                },
                unauthorizedMessage: "Неверное имя пользователя или пароль");
        }

        public Task<RefreshTokenResponse> RefreshToken(string refreshToken, CancellationToken ct)
        {
            return Post<RefreshTokenRequest, RefreshTokenResponse>(
                "/api/auth/tokens/refresh",
                new RefreshTokenRequest
                {
                    RefreshToken = refreshToken
                },
                unauthorizedMessage: "Сессия истекла или была отозвана",
                ct);
        }

        public async Task Logout(string refreshToken, string accessToken)
        {
            using HttpRequestMessage request = new(HttpMethod.Post, "/api/auth/tokens/revoke")
            {
                Content = JsonContent.Create(new LogoutRequest
                {
                    RefreshToken = refreshToken
                }, options: serializerOptions)
            };

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            HttpResponseMessage response = await httpClient.SendAsync(request);

            await EnsureSuccessStatusCode(response, "Сессия уже недействительна");
        }

        private static async Task EnsureSuccessStatusCode(HttpResponseMessage response, string? unauthorizedMessage = null)
        {
            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new InvalidOperationException(unauthorizedMessage ?? "Ошибка авторизации", ex);
            }
            catch (Exception ex)
            {
                string message = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException(message, ex);
            }
        }

        private async Task<TResponse> Post<TRequest, TResponse>(
            string requestUri,
            TRequest request,
            string? unauthorizedMessage = null,
            CancellationToken ct = default)
        {
            HttpResponseMessage response = await httpClient.PostAsJsonAsync(requestUri, request, serializerOptions, ct);

            await EnsureSuccessStatusCode(response, unauthorizedMessage);

            return await response.Content.ReadFromJsonAsync<TResponse>(serializerOptions, ct)
                ?? throw new InvalidOperationException($"Не удалось десериализовать {typeof(TResponse).Name}");
        }
    }
}
