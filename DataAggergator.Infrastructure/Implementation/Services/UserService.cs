using DataAggergator.Application.Abstractions.Services;
using DataAggergator.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAggergator.Infrastructure.Implementation.Services
{
    internal class UserService : IUserService
    {
        private readonly AnalyticsDbContext _context;

        public UserService(AnalyticsDbContext context)
        {
            _context = context;
        }

        public async Task CreateUserAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task<User?> GetUserByKeycloakIdAsync(string keycloakId)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.KeycloakUserId == keycloakId);
        }

        public async Task UpdateLastLoginAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user != null)
            {
                user.LastLoginAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
    }
}
