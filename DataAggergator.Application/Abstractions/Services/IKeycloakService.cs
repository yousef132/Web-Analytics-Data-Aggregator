using DataAggergator.Application.Dtos;

namespace DataAggergator.Application.Abstractions.Services
{
    public interface IKeycloakService
    {
        Task<string> CreateUserAsync(string email, string password, string name);
        Task<TokenResponse> LoginAsync(string email, string password);
        Task<TokenResponse> RefreshTokenAsync(string refreshToken);
        Task LogoutAsync(string keycloakUserId);
    }
}
