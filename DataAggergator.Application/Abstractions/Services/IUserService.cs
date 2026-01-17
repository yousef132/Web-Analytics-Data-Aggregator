using DataAggergator.Domain.Models;

namespace DataAggergator.Application.Abstractions.Services
{
    public interface IUserService
    {
        Task CreateUserAsync(User user);
        Task<User?> GetUserByKeycloakIdAsync(string keycloakId);
        Task UpdateLastLoginAsync(string email);
    }
}
