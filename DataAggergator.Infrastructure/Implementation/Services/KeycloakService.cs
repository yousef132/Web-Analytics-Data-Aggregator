using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using DataAggergator.Application.Abstractions.Services;
using DataAggergator.Application.Dtos;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace DataAggergator.Infrastructure.Implementation.Services
{
    internal class KeycloakService : IKeycloakService
    {

        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _keycloakUrl;
        private readonly string _realm;
        private readonly string _clientId;
        private readonly string _clientSecret;

        public KeycloakService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _keycloakUrl = configuration["Keycloak:BaseUrl"]; // Or from config
            _realm = configuration["Keycloak:Realm"];
            _clientId = configuration["Keycloak:ClientId"];
            _clientSecret = configuration["Keycloak:ClientSecret"]; // Get from Keycloak admin
        }

        public async Task<string> CreateUserAsync(string email, string password, string name)
        {
            // Get admin access token
            var adminToken = await GetAdminTokenAsync();

            var user = new
            {
                username = email,
                email = email,
                firstName = name.Split(' ').FirstOrDefault(),
                lastName = name.Split(' ').Skip(1).FirstOrDefault() ?? "",
                enabled = true,
                emailVerified = true,  // ← IMPORTANT!
                credentials = new[]
                {
                    new
                    {
                        type = "password",
                        value = password,
                        temporary = false
                    }
                },
                requiredActions = new string[] { }  // ← No required actions
            };

            
            var request = new HttpRequestMessage(HttpMethod.Post,
                $"{_keycloakUrl}/admin/realms/{_realm}/users");

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            request.Content = new StringContent(
                JsonSerializer.Serialize(user),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to create user in Keycloak: {error}");
            }

            // Extract user ID from Location header
            var location = response.Headers.Location?.ToString();
            var userId = location?.Split('/').LastOrDefault();

            return userId ?? throw new Exception("Failed to get user ID from Keycloak");
        }

        public async Task<TokenResponse> LoginAsync(string email, string password)
        {
            var content = new FormUrlEncodedContent(new[]
            {
            new KeyValuePair<string, string>("grant_type", "password"),
            new KeyValuePair<string, string>("client_id", _clientId),
            new KeyValuePair<string, string>("client_secret", _clientSecret),
            new KeyValuePair<string, string>("username", email),
            new KeyValuePair<string, string>("password", password),
            new KeyValuePair<string, string>("scope", "openid email")
            });

            var response = await _httpClient.PostAsync(
                $"{_keycloakUrl}/realms/{_realm}/protocol/openid-connect/token",
                content
            );

            if (!response.IsSuccessStatusCode)
            {
                Log.Error("Login Failed !!");
                throw new Exception("Invalid credentials");
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var tokenData = JsonSerializer.Deserialize<JsonElement>(jsonResponse);

            return new TokenResponse
            {
                AccessToken = tokenData.GetProperty("access_token").GetString()!,
                RefreshToken = tokenData.GetProperty("refresh_token").GetString()!,
                ExpiresIn = tokenData.GetProperty("expires_in").GetInt32(),
                TokenType = "Bearer"
            };
        }

        public async Task<TokenResponse> RefreshTokenAsync(string refreshToken)
        {
            var content = new FormUrlEncodedContent(new[]
            {
            new KeyValuePair<string, string>("grant_type", "refresh_token"),
            new KeyValuePair<string, string>("client_id", _clientId),
            new KeyValuePair<string, string>("client_secret", _clientSecret),
            new KeyValuePair<string, string>("refresh_token", refreshToken)
        });

            var response = await _httpClient.PostAsync(
                $"{_keycloakUrl}/realms/{_realm}/protocol/openid-connect/token",
                content
            );

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Invalid refresh token");
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var tokenData = JsonSerializer.Deserialize<JsonElement>(jsonResponse);

            return new TokenResponse
            {
                AccessToken = tokenData.GetProperty("access_token").GetString()!,
                RefreshToken = tokenData.GetProperty("refresh_token").GetString()!,
                ExpiresIn = tokenData.GetProperty("expires_in").GetInt32(),
                TokenType = "Bearer"
            };
        }

        public async Task LogoutAsync(string keycloakUserId)
        {
            var adminToken = await GetAdminTokenAsync();

            var request = new HttpRequestMessage(HttpMethod.Post,
                $"{_keycloakUrl}/admin/realms/{_realm}/users/{keycloakUserId}/logout");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            await _httpClient.SendAsync(request);
        }

        private async Task<string> GetAdminTokenAsync()
        {
            var content = new FormUrlEncodedContent(new[]
            {
            new KeyValuePair<string, string>("grant_type", "password"),
            new KeyValuePair<string, string>("client_id", "admin-cli"),
            new KeyValuePair<string, string>("username", "admin"),
            new KeyValuePair<string, string>("password", "admin")
            });

            var response = await _httpClient.PostAsync(
                $"{_keycloakUrl}/realms/dataaggregator/protocol/openid-connect/token",
                content
            );

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var tokenData = JsonSerializer.Deserialize<JsonElement>(jsonResponse);

            return tokenData.GetProperty("access_token").GetString()!;
        }
    }
}
